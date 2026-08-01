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
    /// Given a member holding a cash bucket
    /// When the projection is run
    /// Then only the part in cash should earn the cash rate
    /// </summary>
    /// <remarks>
    /// The point of the bucket, and what separates it from the earlier model: the balance is not
    /// moved to cash wholesale. Enough to cover the next few years of spending sits in cash and the
    /// rest keeps earning, so the return is a blend weighted by how much each part holds.
    /// </remarks>
    [Fact]
    public void Calculate_HoldingACashBucket_BlendsTheReturn()
    {
        // Arrange: 8% invested, 2% in cash, two years of a 20,000 target held back.
        var plan = TestEntities.CreatePlan(
            expectedReturnRate: 0.08m,
            cashReturnRate: 0.02m,
            cashBucketYears: 2,
            superGuaranteeRate: 0m,
            targetRetirementIncome: 20_000m,
            inflationRate: 0m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, growthStrategy: GrowthStrategy.Custom, accountBalances: [500_000m])]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        // Year 1 is four years from retirement, beyond a two-year bucket, so the whole balance earns 8%.
        Assert.Equal(40_000m, years[1].InvestmentReturn);

        // By year 4 they are one year out, so two years of spending — 40,000 — sits in cash and the
        // rest stays invested.
        var opening = years[4].OpeningBalance;
        var blended = ((40_000m * 0.02m) + ((opening - 40_000m) * 0.08m)) / opening;
        Assert.Equal(Math.Round(opening * blended, 2), years[4].InvestmentReturn, 2);

        // And that is short of the whole balance at 8%, but far nearer it than cash would be.
        Assert.True(years[4].InvestmentReturn < opening * 0.08m);
        Assert.True(years[4].InvestmentReturn > opening * 0.07m);
    }

    /// <summary>
    /// Given a plan holding a cash bucket and one holding none
    /// When both are projected
    /// Then the bucket should cost a little growth, but only on the part held back
    /// </summary>
    /// <remarks>
    /// Protection is not free. What makes the bucket worth having is how little it costs: only the
    /// few years of spending give up the return, not the whole balance.
    /// </remarks>
    [Fact]
    public void Calculate_TheCashBucket_CostsGrowthOnlyOnWhatItHolds()
    {
        // Arrange
        static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan(int bucketYears) =>
            TestEntities.CreatePlan(
                expectedReturnRate: 0.08m,
                cashReturnRate: 0.02m,
                cashBucketYears: bucketYears,
                targetRetirementIncome: 40_000m,
                members: [TestEntities.CreateMember(currentAge: 50, retirementAge: 65, accountBalances: [200_000m])]);

        // Act
        var withBucket = _engine.CalculateWithoutPension(Plan(3), Today).Summary.BalanceAtRetirement;
        var without = _engine.CalculateWithoutPension(Plan(0), Today).Summary.BalanceAtRetirement;

        // Assert
        Assert.True(withBucket < without, $"expected the bucket to cost some growth, but it left {withBucket} against {without}");

        // And the cost is a few per cent, not the third that moving the whole balance would take:
        // only the years of spending gave up the return.
        Assert.True(withBucket > without * 0.9m, $"expected the bucket to cost little, but {withBucket} is far below {without}");
    }
}
