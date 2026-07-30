#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// A projection over the figures actually in the author's plan, checked against arithmetic worked by
/// hand.
/// </summary>
/// <remarks>
/// The rest of the suite uses round numbers so the arithmetic is easy to verify. This one exists to
/// confirm the model behaves sensibly on real inputs — awkward balances, two people retiring in
/// different years, a growth strategy each — where an error in the shape of the model would show up
/// as an implausible figure rather than a failed assertion about a round number.
/// </remarks>
[Trait("Category", "Unit")]
public class RealDataSanityTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private readonly RetirementProjectionEngine _engine = new();

    private static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan(decimal targetIncome) =>
        TestEntities.CreatePlan(
            expectedReturnRate: 0.065m,
            inflationRate: 0.025m,
            superGuaranteeRate: 0.12m,
            contributionsTaxRate: 0.15m,
            lifeExpectancy: 90,
            targetRetirementIncome: targetIncome,
            preRetirementSwitchYears: 2,
            cashReturnRate: 0.03m,
            members: [
                TestEntities.CreateMember(name: "Andy", currentAge: 47, retirementAge: 67, currentIncome: 231_000m,
                    salarySacrifice: 1_000m, growthStrategy: GrowthStrategy.Growth, accountBalances: [511_538.57m]),
                TestEntities.CreateMember(name: "Margo", currentAge: 51, retirementAge: 67, currentIncome: 180_000m,
                    salarySacrifice: 0m, growthStrategy: GrowthStrategy.Balanced, accountBalances: [146_500.59m]),
            ]);

    /// <summary>
    /// Given the real plan with a 90,000 target
    /// When the projection is run
    /// Then it should reach retirement, draw the target, and last to life expectancy
    /// </summary>
    [Fact]
    public void Calculate_TheRealPlan_HoldsUp()
    {
        // Act
        var projection = _engine.CalculateWithoutPension(Plan(90_000m), Today);
        var years = projection.Years.ToList();
        var summary = projection.Summary;

        // Assert
        // Andy is the younger, so the household's last retirement is his, twenty years out.
        Assert.Equal(2046, summary.RetirementYear);
        Assert.Equal(2069, summary.LifeExpectancyYear);

        // Nothing drawn until the year after they have both retired.
        Assert.Equal(0m, years[20].Drawdown);
        Assert.True(years[21].Drawdown > 0m);

        // The balance peaks at retirement and falls from there.
        Assert.True(summary.BalanceAtRetirement > years[^1].ClosingBalance);
        Assert.True(years[25].ClosingBalance < years[20].ClosingBalance);

        // A 90,000 target in today's dollars, indexed twenty-one years at 2.5%, is about 151,000.
        var expectedFirstDraw = 90_000m * (decimal)Math.Pow(1.025d, 21);
        Assert.Equal(expectedFirstDraw, years[21].Drawdown, 0);

        // Which is still 90,000 of buying money.
        Assert.Equal(90_000m, years[21].DrawdownInTodaysDollars, 0);
    }

    /// <summary>
    /// Given the real plan
    /// When the target income is raised
    /// Then the money should run out sooner
    /// </summary>
    /// <remarks>
    /// The relationship the whole feature is for. Checked as a direction rather than a figure, so it
    /// does not need updating every time an assumption is tuned.
    /// </remarks>
    [Fact]
    public void Calculate_ARaisedTarget_RunsOutSooner()
    {
        // Act
        var modest = _engine.CalculateWithoutPension(Plan(60_000m), Today).Summary;
        var comfortable = _engine.CalculateWithoutPension(Plan(150_000m), Today).Summary;
        var extravagant = _engine.CalculateWithoutPension(Plan(400_000m), Today).Summary;

        // Assert
        Assert.Null(modest.MoneyRunsOutYear);
        Assert.NotNull(extravagant.MoneyRunsOutYear);

        if (comfortable.MoneyRunsOutYear is not null)
        {
            Assert.True(comfortable.MoneyRunsOutYear > extravagant.MoneyRunsOutYear);
        }
    }

    /// <summary>
    /// Given the real plan with the seeded pension rates
    /// When the projection is run
    /// Then the pension should start at nothing and appear only as the balances deplete
    /// </summary>
    /// <remarks>
    /// The shape a superannuation calculator shows, checked on real figures: the household is far
    /// above the assets cut-off at retirement, so it qualifies for nothing at first, and the pension
    /// only appears once enough has been spent to bring it under the threshold.
    /// </remarks>
    [Fact]
    public void Calculate_TheRealPlanWithAPension_QualifiesOnlyAsTheBalanceFalls()
    {
        // Arrange: rates approximating the seeded homeowner figures.
        var rates = new AgePensionRates(67, 29_900m, 45_080m, 314_000m, 470_000m, 0.078m);

        // Act
        var projection = _engine.Calculate(Plan(150_000m), Today, rates);
        var drawing = projection.Years.Where(y => y.TotalIncome > 0m).ToList();

        // Assert
        Assert.NotEmpty(drawing);
        Assert.Equal(0m, drawing[0].Pension);

        // The pension never falls back once it starts, because the balance only goes down.
        for (var i = 1; i < drawing.Count; i++)
        {
            Assert.True(drawing[i].Pension >= drawing[i - 1].Pension,
                $"pension fell from {drawing[i - 1].Pension} to {drawing[i].Pension} in {drawing[i].Year}");
        }

        // And by the end it is carrying part of the income.
        Assert.True(projection.Summary.TotalPension > 0m);
    }

    /// <summary>
    /// Given the real plan
    /// When the projection is run
    /// Then both people should fund the income, the larger balance carrying more of it
    /// </summary>
    [Fact]
    public void Calculate_TheRealPlan_SplitsTheIncomeBetweenBothPeople()
    {
        // Act
        var firstDrawYear = _engine.CalculateWithoutPension(Plan(90_000m), Today).Years.ElementAt(21);

        // Assert
        var andy = firstDrawYear.Members.Single(m => m.Name == "Andy");
        var margo = firstDrawYear.Members.Single(m => m.Name == "Margo");

        Assert.True(andy.Drawdown > 0m);
        Assert.True(margo.Drawdown > 0m);
        Assert.True(andy.Drawdown > margo.Drawdown, "Andy holds the larger balance, so he should fund more of the income");
        Assert.Equal(firstDrawYear.Drawdown, andy.Drawdown + margo.Drawdown);
    }
}
