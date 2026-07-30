#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the phase after retirement: the switch to cash, the drawdown, and whether the
/// money lasts.
/// </summary>
[Trait("Category", "Unit")]
public class RetirementDrawdownTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// Given a member who retires in five years and a target income
    /// When the projection is run
    /// Then nothing should be drawn until the year after they retire
    /// </summary>
    /// <remarks>
    /// The retirement year itself is still a working year in this model — they contribute in it — so
    /// drawing in the same year would have them earning and spending their savings at once.
    /// </remarks>
    [Fact]
    public void Calculate_TheRetirementYear_DrawsNothing()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            targetRetirementIncome: 40_000m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [500_000m])]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(0m, years[5].Drawdown);
        Assert.Equal(40_000m, years[6].Drawdown);
    }

    /// <summary>
    /// Given a household drawing an income
    /// When the projection is run
    /// Then the balance should fall rather than keep growing
    /// </summary>
    /// <remarks>
    /// The behaviour the drawdown phase exists for. With no drawdown the balance compounded away to
    /// life expectancy, which is not what retirement looks like.
    /// </remarks>
    [Fact]
    public void Calculate_DuringDrawdown_TheBalanceFalls()
    {
        // Arrange: a target well above what a 3% cash return on the balance yields.
        var plan = TestEntities.CreatePlan(
            expectedReturnRate: 0.06m,
            cashReturnRate: 0.03m,
            targetRetirementIncome: 60_000m,
            members: [TestEntities.CreateMember(currentAge: 64, retirementAge: 65, currentIncome: 0m, accountBalances: [800_000m])]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        var atRetirement = years[1].ClosingBalance;
        Assert.True(years[2].ClosingBalance < atRetirement, $"expected the balance to fall, but it went from {atRetirement} to {years[2].ClosingBalance}");
        Assert.True(years[^1].ClosingBalance < atRetirement);
    }

    /// <summary>
    /// Given a target income the balance cannot sustain
    /// When the projection is run
    /// Then the year it falls short should be reported and the balance should not go negative
    /// </summary>
    [Fact]
    public void Calculate_ATargetTheBalanceCannotSustain_ReportsTheYearItRunsOut()
    {
        // Arrange: 100,000 has to fund 50,000 a year, so it lasts about two years.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 50_000m,
            members: [TestEntities.CreateMember(currentAge: 65, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m])]);

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today);
        var years = projection.Years.ToList();

        // Assert
        Assert.Equal(2026 + 3, projection.Summary.MoneyRunsOutYear);
        Assert.Equal(0m, projection.Summary.FinalBalance);
        Assert.All(years, y => Assert.True(y.ClosingBalance >= 0m, $"balance went negative in {y.Year}"));
    }

    /// <summary>
    /// Given a target income the balance can sustain
    /// When the projection is run
    /// Then no shortfall year should be reported
    /// </summary>
    [Fact]
    public void Calculate_ASustainableTarget_ReportsNoShortfall()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            lifeExpectancy: 90,
            targetRetirementIncome: 20_000m,
            members: [TestEntities.CreateMember(currentAge: 65, retirementAge: 65, currentIncome: 0m, accountBalances: [2_000_000m])]);

        // Act
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert
        Assert.Null(summary.MoneyRunsOutYear);
        Assert.True(summary.FinalBalance > 0m);
    }

    /// <summary>
    /// Given a plan that targets no income
    /// When the projection is run
    /// Then it should never be reported as running out
    /// </summary>
    /// <remarks>
    /// A plan whose target has not been set yet is incomplete, not doomed; it must not show a
    /// shortfall warning.
    /// </remarks>
    [Fact]
    public void Calculate_NoTargetIncome_NeverRunsOut()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            targetRetirementIncome: 0m,
            members: [TestEntities.CreateMember(currentAge: 64, retirementAge: 65, accountBalances: [1_000m])]);

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today);

        // Assert
        Assert.Null(projection.Summary.MoneyRunsOutYear);
        Assert.All(projection.Years, y => Assert.Equal(0m, y.Drawdown));
    }

    /// <summary>
    /// Given two members with different balances
    /// When the household draws its income
    /// Then each should fund it in proportion to what they hold
    /// </summary>
    [Fact]
    public void Calculate_TwoMembers_ShareTheDrawdownByBalance()
    {
        // Arrange: a 3:1 split of the balances.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 40_000m,
            members: [
                TestEntities.CreateMember(name: "Bigger", currentAge: 65, retirementAge: 65, currentIncome: 0m, accountBalances: [300_000m]),
                TestEntities.CreateMember(name: "Smaller", currentAge: 65, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m]),
            ]);

        // Act
        var firstDrawdownYear = _engine.CalculateWithoutPension(plan, Today).Years.ElementAt(1);

        // Assert
        Assert.Equal(40_000m, firstDrawdownYear.Drawdown);
        Assert.Equal(30_000m, firstDrawdownYear.Members.Single(m => m.Name == "Bigger").Drawdown);
        Assert.Equal(10_000m, firstDrawdownYear.Members.Single(m => m.Name == "Smaller").Drawdown);
    }

    /// <summary>
    /// Given one member still working and another already retired
    /// When the projection is run
    /// Then nothing should be drawn while the household still has an earner
    /// </summary>
    [Fact]
    public void Calculate_OneMemberStillWorking_DrawsNothingYet()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            targetRetirementIncome: 50_000m,
            members: [
                TestEntities.CreateMember(currentAge: 65, retirementAge: 65, accountBalances: [400_000m]),
                TestEntities.CreateMember(currentAge: 55, retirementAge: 65, accountBalances: [400_000m]),
            ]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        // Ten years until the younger one retires, then drawing starts the year after.
        Assert.All(years.Take(11), y => Assert.Equal(0m, y.Drawdown));
        Assert.True(years[11].Drawdown > 0m);
    }

    /// <summary>
    /// Given a member whose balance moves to cash before they retire
    /// When the projection is run
    /// Then the years inside the switch window should earn the cash rate
    /// </summary>
    [Fact]
    public void Calculate_WithinTheSwitchWindow_EarnsTheCashRate()
    {
        // Arrange: 8% invested, 2% in cash, switching two years out.
        var plan = TestEntities.CreatePlan(
            expectedReturnRate: 0.08m,
            cashReturnRate: 0.02m,
            preRetirementSwitchYears: 2,
            superGuaranteeRate: 0m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, growthStrategy: GrowthStrategy.Custom, accountBalances: [100_000m])]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        // Years 1 and 2 are still invested; from year 3 they are two years out and in cash.
        Assert.Equal(8_000m, years[1].InvestmentReturn);
        Assert.Equal(Math.Round(years[2].OpeningBalance * 0.08m, 2), years[2].InvestmentReturn);
        Assert.Equal(Math.Round(years[3].OpeningBalance * 0.02m, 2), years[3].InvestmentReturn);
        Assert.Equal(Math.Round(years[5].OpeningBalance * 0.02m, 2), years[5].InvestmentReturn);
    }

    /// <summary>
    /// Given a plan that switches to cash and one that does not
    /// When both are projected
    /// Then the switch should leave less at retirement
    /// </summary>
    /// <remarks>
    /// The switch is protection, not free: it gives up the higher return for the years it applies to.
    /// Worth pinning, because a glide that made no difference would mean it was not being applied.
    /// </remarks>
    [Fact]
    public void Calculate_TheSwitchToCash_CostsGrowthBeforeRetirement()
    {
        // Arrange
        static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan(int switchYears) =>
            TestEntities.CreatePlan(
                expectedReturnRate: 0.08m,
                cashReturnRate: 0.02m,
                preRetirementSwitchYears: switchYears,
                members: [TestEntities.CreateMember(currentAge: 50, retirementAge: 65, accountBalances: [200_000m])]);

        // Act
        var withGlide = _engine.CalculateWithoutPension(Plan(5), Today).Summary.BalanceAtRetirement;
        var withoutGlide = _engine.CalculateWithoutPension(Plan(0), Today).Summary.BalanceAtRetirement;

        // Assert
        Assert.True(withGlide < withoutGlide, $"expected the glide to cost growth, but it left {withGlide} against {withoutGlide}");
    }
}
