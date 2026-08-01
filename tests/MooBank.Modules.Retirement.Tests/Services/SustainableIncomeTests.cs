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
        var sustainable = _engine.CalculateWithoutPension(plan, Today).Summary.SustainableIncomeInTodaysDollars;

        plan.TargetRetirementIncome = sustainable;
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert
        Assert.True(sustainable > 0m);
        Assert.Null(summary.MoneyRunsOutYear);
    }
}
