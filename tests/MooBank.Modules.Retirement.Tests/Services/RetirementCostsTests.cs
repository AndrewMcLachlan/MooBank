#nullable enable
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for fund fees and insurance premiums.
/// </summary>
/// <remarks>
/// The default test plan uses a 10% return, no inflation, a 10% employer contribution and no
/// contributions tax, so every figure below can be checked by hand.
/// </remarks>
[Trait("Category", "Unit")]
public class RetirementCostsTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// Given a member charged fees and an insurance premium
    /// When the projection is run
    /// Then both should come out of the balance for the year
    /// </summary>
    [Fact]
    public void Calculate_FeesAndInsurance_AreBothDeducted()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, annualFees: 372m, insurancePremium: 364m, accountBalances: [100_000m]),
        ]);

        // Act
        var firstYear = _engine.Calculate(plan, Today).Years.ElementAt(1);

        // Assert
        Assert.Equal(736m, firstYear.Costs);
        // 100,000 opening, 10,000 return, less 736 of costs.
        Assert.Equal(109_264m, firstYear.ClosingBalance);
    }

    /// <summary>
    /// Given no fees or premiums
    /// When the projection is run
    /// Then no costs should be reported
    /// </summary>
    [Fact]
    public void Calculate_NoFeesOrInsurance_CostsNothing()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.All(projection.Years, y => Assert.Equal(0m, y.Costs));
        Assert.Equal(0m, projection.Summary.TotalCosts);
    }

    /// <summary>
    /// Given fees charged every year
    /// When the projection is run
    /// Then the total reported should be the sum of every year's costs
    /// </summary>
    [Fact]
    public void Calculate_Costs_AreTotalledAcrossTheProjection()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, annualFees: 400m, insurancePremium: 100m, accountBalances: [100_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        // Five projected years at 500 a year; the starting-position row carries no costs.
        // 500 a year for the 30 years from 60 to the plan's life expectancy of 90. Fees are charged
        // through retirement too, not only while contributing.
        Assert.Equal(15_000m, projection.Summary.TotalCosts);
        Assert.Equal(projection.Years.Sum(y => y.Costs), projection.Summary.TotalCosts);
    }

    /// <summary>
    /// Given fees under inflation
    /// When the projection is run
    /// Then they should be indexed like income, so they hold their real value
    /// </summary>
    [Fact]
    public void Calculate_WithInflation_IndexesCosts()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0.02m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, annualFees: 1_000m, accountBalances: [100_000m])]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(1_000m, years[1].Costs);
        Assert.Equal(1_020m, years[2].Costs);
    }

    /// <summary>
    /// Given fees charged year by year
    /// When compared against the same total taken as a lump at the end
    /// Then the year-by-year balance should be lower, because the fees lose their compounding too
    /// </summary>
    /// <remarks>
    /// This is the substance of deducting inside the loop rather than subtracting
    /// fees × years at the end: an early fee costs its own value plus everything it would have
    /// earned. Taking the total off at the end understates the drag.
    /// </remarks>
    [Fact]
    public void Calculate_FeesDeductedYearly_CostMoreThanTheirFaceValue()
    {
        // Arrange
        var withFees = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 45, retirementAge: 65, currentIncome: 0m, annualFees: 1_000m, accountBalances: [100_000m]),
        ]);
        var withoutFees = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 45, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m]),
        ]);

        // Act
        var withFeesResult = _engine.Calculate(withFees, Today);
        var withoutFeesResult = _engine.Calculate(withoutFees, Today);

        // Assert
        var difference = withoutFeesResult.Summary.BalanceAtRetirement - withFeesResult.Summary.BalanceAtRetirement;

        // 20 years at 1,000 is 20,000 of face value; the lost growth makes the real cost far more.
        // 1,000 a year from 45 to the life expectancy of 90.
        Assert.Equal(45_000m, withFeesResult.Summary.TotalCosts);
        Assert.True(difference > 20_000m, $"expected the drag to exceed the fees' face value, but it was {difference}");
    }

    /// <summary>
    /// Given a balance too small to cover its fees
    /// When the projection is run
    /// Then the balance should stop at nothing rather than going negative
    /// </summary>
    [Fact]
    public void Calculate_FeesLargerThanTheBalance_DoNotDriveItNegative()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, annualFees: 10_000m, accountBalances: [100m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.All(projection.Years, y => Assert.True(y.ClosingBalance >= 0m, $"balance went negative: {y.ClosingBalance}"));
    }

    /// <summary>
    /// Given a member who has reached their retirement age while another is still working
    /// When the projection continues
    /// Then their fees should keep being charged
    /// </summary>
    /// <remarks>
    /// Contributions stop at retirement; fees do not, because the account still exists.
    /// </remarks>
    [Fact]
    public void Calculate_AfterRetirement_FeesKeepBeingCharged()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "Early", currentAge: 60, retirementAge: 61, currentIncome: 0m, annualFees: 500m, accountBalances: [100_000m]),
            TestEntities.CreateMember(name: "Late", currentAge: 60, retirementAge: 65, currentIncome: 0m, accountBalances: [0m]),
        ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(500m, years[1].Costs);
        Assert.Equal(500m, years[3].Costs);
    }
}
