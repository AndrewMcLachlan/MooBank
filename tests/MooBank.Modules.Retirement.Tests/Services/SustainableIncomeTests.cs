#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the sustainable income reported on the summary — the figure the target income
/// slider is solved against.
/// </summary>
/// <remarks>
/// The web app solves the target income and the life expectancy from each other using the same
/// annuity on the same three inputs, so that the two sliders and this figure all agree. That makes
/// the arithmetic here a contract rather than an implementation detail: the values below are the ones
/// <c>retirementSync.ts</c> must reproduce, and its own tests assert the mirror image.
/// </remarks>
[Trait("Category", "Unit")]
public class SustainableIncomeTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// Given a balance, a real return and a horizon
    /// When the sustainable income is worked out
    /// Then it should be the annuity payment those three imply
    /// </summary>
    [Fact]
    public void AnnualDrawdown_IsTheAnnuityPayment()
    {
        // 1,000,000 over 20 years at a 4% real return.
        var expected = (decimal)(1_000_000d * 0.04d / (1d - Math.Pow(1.04d, -20)));

        Assert.Equal(expected, RetirementProjectionEngine.AnnualDrawdown(1_000_000m, 0.04m, 20), 2);
    }

    /// <summary>
    /// Given no real return
    /// When the sustainable income is worked out
    /// Then the balance should simply be spread across the years
    /// </summary>
    [Fact]
    public void AnnualDrawdown_WithNoRealReturn_SpreadsEvenly()
    {
        Assert.Equal(50_000m, RetirementProjectionEngine.AnnualDrawdown(1_000_000m, 0m, 20));
    }

    /// <summary>
    /// Given a projection
    /// When the summary is read
    /// Then the sustainable income should be worked out on the household, not summed from the members
    /// </summary>
    /// <remarks>
    /// The two bases give different answers — each member has their own return and retirement age —
    /// and a target income is a household figure. Summing the members here would have the page report
    /// a sustainable income that contradicted the target beside it.
    /// </remarks>
    [Fact]
    public void Calculate_TheSummaryFigure_IsWorkedOutOnTheHousehold()
    {
        // Arrange: two members on different strategies, so the two bases cannot coincide.
        var plan = TestEntities.CreatePlan(
            expectedReturnRate: 0.065m,
            inflationRate: 0.025m,
            lifeExpectancy: 90,
            members: [
                TestEntities.CreateMember(currentAge: 60, retirementAge: 67, growthStrategy: GrowthStrategy.Growth, accountBalances: [500_000m]),
                TestEntities.CreateMember(currentAge: 62, retirementAge: 65, growthStrategy: GrowthStrategy.Conservative, accountBalances: [200_000m]),
            ]);

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today);
        var summary = projection.Summary;

        // Assert
        var expected = RetirementProjectionEngine.AnnualDrawdown(
            summary.BalanceAtRetirementInTodaysDollars,
            RetirementProjectionEngine.RealReturnRate(0.065m, 0.025m),
            summary.LifeExpectancyYear - summary.RetirementYear);

        Assert.Equal(expected, summary.AnnualRetirementIncomeInTodaysDollars);

        // And it is genuinely not the sum of the members' own figures.
        Assert.NotEqual(projection.Members.Sum(m => m.AnnualRetirementIncomeInTodaysDollars), summary.AnnualRetirementIncomeInTodaysDollars);
    }

    /// <summary>
    /// Given a projection
    /// When the summary is read
    /// Then the retirement age should be the last member's, which is where the drawdown starts
    /// </summary>
    /// <remarks>
    /// Stated on the summary because the web app needs it to solve the horizon while holding unsaved
    /// slider values, when it cannot reconstruct it from the years.
    /// </remarks>
    [Fact]
    public void Calculate_TheRetirementAge_IsTheLastToRetire()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [100_000m]),
            TestEntities.CreateMember(currentAge: 50, retirementAge: 70, accountBalances: [100_000m]),
        ]);

        // Act
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert
        Assert.Equal(70, summary.RetirementAge);
        // Twenty years until the younger one reaches 70.
        Assert.Equal(2026 + 20, summary.RetirementYear);
    }

    /// <summary>
    /// Given the horizon and the income solved from each other
    /// When both are applied to a projection
    /// Then the plan should not be reported as running out
    /// </summary>
    /// <remarks>
    /// The promise the two linked sliders make. Checked without a pension, which is the basis the
    /// solve uses — a pension only ever adds income, so it can push the money out further but never
    /// shorter.
    /// </remarks>
    [Fact]
    public void Calculate_ATargetSolvedFromTheHorizon_Lasts()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            expectedReturnRate: 0.065m,
            inflationRate: 0.025m,
            lifeExpectancy: 90,
            members: [TestEntities.CreateMember(currentAge: 55, retirementAge: 67, accountBalances: [600_000m])]);

        // Act: read the sustainable income off a first pass, then run again targeting it.
        var sustainable = _engine.CalculateWithoutPension(plan, Today).Summary.AnnualRetirementIncomeInTodaysDollars;

        plan.TargetRetirementIncome = sustainable;
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert
        Assert.True(sustainable > 0m);
        Assert.Null(summary.MoneyRunsOutYear);
    }
}
