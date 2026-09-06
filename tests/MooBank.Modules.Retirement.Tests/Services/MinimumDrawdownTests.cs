using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// The legislated floor on what an account-based pension pays out.
/// </summary>
[Trait("Category", "Unit")]
public class MinimumDrawdownTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    private readonly RetirementProjectionEngine _engine = new();

    private static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan(decimal target) =>
        TestEntities.CreatePlan(
            expectedReturnRate: 0m, inflationRate: 0m, superGuaranteeRate: 0m, lifeExpectancy: 75,
            targetRetirementIncome: target,
            members: [TestEntities.CreateMember(currentAge: 64, retirementAge: 65, currentIncome: 0m, accountBalances: [1_000_000m])]);

    private RetirementProjection Run(decimal target) =>
        _engine.Calculate(Plan(target), Today, AgePensionRates.None, TestEntities.StrategyRates(Plan(target)), TestEntities.MinimumDrawdown());

    /// <summary>
    /// Given a target well below the legislated minimum
    /// When the projection reaches the drawdown phase
    /// Then the minimum should be drawn anyway
    /// </summary>
    /// <remarks>
    /// The money leaves superannuation whether or not the household means to spend it, which is why
    /// a modest target does not simply let a large balance compound.
    /// </remarks>
    [Fact]
    public void Calculate_ATargetBelowTheMinimum_DrawsTheMinimum()
    {
        // Act: first drawdown year, at 65, where Schedule 7 asks for 5%.
        var firstDraw = Run(10_000m).Years.ElementAt(2);

        // Assert: 5% of the million it opened with, not the 10,000 the plan wanted.
        Assert.Equal(50_000m, firstDraw.Drawdown);
    }

    /// <summary>
    /// Given a target above the legislated minimum
    /// When the projection reaches the drawdown phase
    /// Then the target should be drawn, not the minimum
    /// </summary>
    [Fact]
    public void Calculate_ATargetAboveTheMinimum_DrawsTheTarget()
    {
        var firstDraw = Run(80_000m).Years.ElementAt(2);

        Assert.Equal(80_000m, firstDraw.Drawdown);
    }

    /// <summary>
    /// Given the drawdown bands
    /// When a rate is asked for
    /// Then it should be the band the age falls in
    /// </summary>
    [Theory]
    [InlineData(60, 0.04)]
    [InlineData(65, 0.05)]
    [InlineData(74, 0.05)]
    [InlineData(75, 0.06)]
    [InlineData(84, 0.07)]
    [InlineData(95, 0.14)]
    [InlineData(120, 0.14)]
    public void For_AnAge_TakesTheBandItFallsIn(int age, decimal expected) =>
        Assert.Equal(expected, TestEntities.MinimumDrawdown().For(age));

    /// <summary>
    /// Given no rates recorded
    /// When a rate is asked for
    /// Then nothing is forced out
    /// </summary>
    /// <remarks>
    /// A missing table is a settings gap, not a reason to refuse to project — the same treatment
    /// the Age Pension rates get.
    /// </remarks>
    [Fact]
    public void For_NoRates_ForcesNothingOut() =>
        Assert.Equal(0m, MinimumDrawdownRates.None.For(80));
}
