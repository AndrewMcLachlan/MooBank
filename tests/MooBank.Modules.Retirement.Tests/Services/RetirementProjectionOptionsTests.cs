#nullable enable
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for salary sacrifice, growth strategies and the tweak-slider overrides.
/// </summary>
/// <remarks>
/// The default test plan uses a 10% return, no inflation, a 10% employer contribution and no
/// contributions tax, so every figure below can be checked by hand.
/// </remarks>
[Trait("Category", "Unit")]
public class RetirementProjectionOptionsTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    private readonly RetirementProjectionEngine _engine = new();

    // ---- Salary sacrifice ----

    /// <summary>
    /// Given a member who salary sacrifices
    /// When the projection is run
    /// Then the sacrifice should be added to the employer contribution
    /// </summary>
    [Fact]
    public void Calculate_WithSalarySacrifice_AddsItToTheEmployerContribution()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 100_000m, salarySacrifice: 5_000m, accountBalances: [0m]),
        ]);

        // Act
        var firstYear = _engine.Calculate(plan, Today).Years.ElementAt(1);

        // Assert
        // 100,000 * 10% employer, plus 5,000 sacrificed.
        Assert.Equal(15_000m, firstYear.Contributions);
    }

    /// <summary>
    /// Given a member who salary sacrifices under contributions tax
    /// When the projection is run
    /// Then the sacrifice should be taxed the same as the employer contribution
    /// </summary>
    /// <remarks>
    /// Salary sacrifice is a concessional contribution, so it attracts contributions tax on the way
    /// into the fund exactly as the employer's does.
    /// </remarks>
    [Fact]
    public void Calculate_SalarySacrificeUnderContributionsTax_IsTaxedLikeTheEmployerContribution()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            contributionsTaxRate: 0.15m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 100_000m, salarySacrifice: 5_000m, accountBalances: [0m])]);

        // Act
        var firstYear = _engine.Calculate(plan, Today).Years.ElementAt(1);

        // Assert
        // (10,000 + 5,000) * 85%
        Assert.Equal(12_750m, firstYear.Contributions);
    }

    /// <summary>
    /// Given inflation
    /// When the projection is run
    /// Then salary sacrifice should be indexed alongside income so it holds its real value
    /// </summary>
    [Fact]
    public void Calculate_WithInflation_IndexesSalarySacrifice()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0.02m,
            members: [TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, salarySacrifice: 10_000m, accountBalances: [0m])]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(10_000m, years[1].Contributions);
        Assert.Equal(10_200m, years[2].Contributions);
    }

    /// <summary>
    /// Given a member past their retirement age
    /// When the projection is run
    /// Then their salary sacrifice should stop with their employer contributions
    /// </summary>
    [Fact]
    public void Calculate_AfterRetirement_SalarySacrificeStops()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "Early", currentAge: 60, retirementAge: 61, currentIncome: 0m, salarySacrifice: 10_000m, accountBalances: [0m]),
            TestEntities.CreateMember(name: "Late", currentAge: 60, retirementAge: 65, currentIncome: 0m, salarySacrifice: 0m, accountBalances: [0m]),
        ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(10_000m, years[1].Contributions);
        Assert.Equal(0m, years[2].Contributions);
    }

    // ---- Growth strategies ----

    /// <summary>
    /// Given a named growth strategy
    /// When its return rate is resolved
    /// Then the strategy's own rate should be used rather than the plan's
    /// </summary>
    [Theory]
    [InlineData(GrowthStrategy.Conservative, 0.045)]
    [InlineData(GrowthStrategy.Balanced, 0.060)]
    [InlineData(GrowthStrategy.Growth, 0.070)]
    [InlineData(GrowthStrategy.HighGrowth, 0.080)]
    public void ReturnRateFor_NamedStrategy_UsesItsOwnRate(GrowthStrategy strategy, decimal expected)
    {
        // Act
        var rate = RetirementProjectionEngine.ReturnRateFor(strategy, planRate: 0.99m);

        // Assert
        Assert.Equal(expected, rate);
    }

    /// <summary>
    /// Given the custom growth strategy
    /// When its return rate is resolved
    /// Then the plan's own rate should be used
    /// </summary>
    [Fact]
    public void ReturnRateFor_Custom_FallsBackToThePlanRate()
    {
        // Act
        var rate = RetirementProjectionEngine.ReturnRateFor(GrowthStrategy.Custom, planRate: 0.0625m);

        // Assert
        Assert.Equal(0.0625m, rate);
    }

    /// <summary>
    /// Given two members on different growth strategies
    /// When the projection is run
    /// Then each balance should grow at its own rate
    /// </summary>
    [Fact]
    public void Calculate_MembersOnDifferentStrategies_EachGrowsAtItsOwnRate()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "Cautious", currentAge: 60, retirementAge: 65, currentIncome: 0m, growthStrategy: GrowthStrategy.Conservative, accountBalances: [100_000m]),
            TestEntities.CreateMember(name: "Bold", currentAge: 60, retirementAge: 65, currentIncome: 0m, growthStrategy: GrowthStrategy.HighGrowth, accountBalances: [100_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        // 4.5% on 100,000 plus 8% on 100,000.
        Assert.Equal(12_500m, projection.Years.ElementAt(1).InvestmentReturn);

        var cautious = projection.Members.Single(m => m.Name == "Cautious");
        var bold = projection.Members.Single(m => m.Name == "Bold");
        Assert.Equal(0.045m, cautious.ReturnRate);
        Assert.Equal(0.080m, bold.ReturnRate);
        Assert.True(bold.BalanceAtRetirement > cautious.BalanceAtRetirement);
    }

    /// <summary>
    /// Given a member on a named strategy
    /// When their outcome is produced
    /// Then it should report the strategy it was projected under
    /// </summary>
    [Fact]
    public void Calculate_MemberOutcome_ReportsTheStrategyUsed()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, growthStrategy: GrowthStrategy.Growth, accountBalances: [100_000m]),
        ]);

        // Act
        var member = _engine.Calculate(plan, Today).Members.Single();

        // Assert
        Assert.Equal(GrowthStrategy.Growth, member.GrowthStrategy);
        Assert.Equal(0.070m, member.ReturnRate);
    }

    // ---- Overrides ----

    /// <summary>
    /// Given an override for a member's retirement age
    /// When the projection is run
    /// Then it should run to the overridden age
    /// </summary>
    [Fact]
    public void Calculate_OverridingRetirementAge_ChangesTheHorizon()
    {
        // Arrange
        var member = TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [100_000m]);
        var plan = TestEntities.CreatePlan(members: [member]);

        var overrides = new ProjectionOverrides
        {
            Members = [new MemberOverride { MemberId = member.Id, RetirementAge = 70 }],
        };

        // Act
        var projection = _engine.Calculate(plan, Today, overrides);

        // Assert
        Assert.Equal(2036, projection.Summary.RetirementYear);
        Assert.Equal(10, projection.Members.Single().YearsToRetirement);
    }

    /// <summary>
    /// Given an override for a member's income
    /// When the projection is run
    /// Then contributions should follow the overridden income
    /// </summary>
    [Fact]
    public void Calculate_OverridingIncome_ChangesContributions()
    {
        // Arrange
        var member = TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 100_000m, accountBalances: [0m]);
        var plan = TestEntities.CreatePlan(members: [member]);

        var overrides = new ProjectionOverrides
        {
            Members = [new MemberOverride { MemberId = member.Id, CurrentIncome = 150_000m }],
        };

        // Act
        var firstYear = _engine.Calculate(plan, Today, overrides).Years.ElementAt(1);

        // Assert
        Assert.Equal(15_000m, firstYear.Contributions);
    }

    /// <summary>
    /// Given plan-level overrides
    /// When the projection is run
    /// Then the overridden rates should be used instead of the plan's
    /// </summary>
    [Fact]
    public void Calculate_OverridingPlanRates_UsesTheOverriddenRates()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m]),
        ]);

        var overrides = new ProjectionOverrides { ExpectedReturnRate = 0.20m };

        // Act
        var firstYear = _engine.Calculate(plan, Today, overrides).Years.ElementAt(1);

        // Assert
        Assert.Equal(20_000m, firstYear.InvestmentReturn);
    }

    /// <summary>
    /// Given an override
    /// When the projection is run
    /// Then the saved plan should be left untouched
    /// </summary>
    /// <remarks>
    /// This is the guarantee behind the tweak sliders. Overrides that leaked onto the entity would
    /// be written out by the next unrelated save.
    /// </remarks>
    [Fact]
    public void Calculate_WithOverrides_DoesNotMutateThePlan()
    {
        // Arrange
        var member = TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 100_000m, salarySacrifice: 0m, accountBalances: [100_000m]);
        var plan = TestEntities.CreatePlan(expectedReturnRate: 0.10m, members: [member]);

        var overrides = new ProjectionOverrides
        {
            ExpectedReturnRate = 0.20m,
            InflationRate = 0.09m,
            LifeExpectancy = 100,
            Members = [new MemberOverride { MemberId = member.Id, RetirementAge = 70, CurrentIncome = 999_999m, SalarySacrifice = 50_000m, CurrentAge = 30, GrowthStrategy = GrowthStrategy.HighGrowth }],
        };

        // Act
        _engine.Calculate(plan, Today, overrides);

        // Assert
        Assert.Equal(0.10m, plan.ExpectedReturnRate);
        Assert.Equal(90, plan.LifeExpectancy);
        Assert.Equal(65, member.RetirementAge);
        Assert.Equal(100_000m, member.CurrentIncome);
        Assert.Equal(0m, member.SalarySacrifice);
        Assert.Equal(60, member.CurrentAge);
        Assert.Equal(GrowthStrategy.Custom, member.GrowthStrategy);
    }

    /// <summary>
    /// Given an override naming a member who is not on the plan
    /// When the projection is run
    /// Then it should be ignored rather than fail
    /// </summary>
    /// <remarks>
    /// A slider left over from a member who has since been removed must not be able to break the
    /// whole projection.
    /// </remarks>
    [Fact]
    public void Calculate_OverrideForAnUnknownMember_IsIgnored()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [100_000m]),
        ]);

        var overrides = new ProjectionOverrides
        {
            Members = [new MemberOverride { MemberId = Guid.NewGuid(), RetirementAge = 80 }],
        };

        // Act
        var projection = _engine.Calculate(plan, Today, overrides);

        // Assert
        Assert.Equal(2031, projection.Summary.RetirementYear);
    }

    /// <summary>
    /// Given no overrides at all
    /// When the projection is run
    /// Then it should match a run with an empty override object
    /// </summary>
    [Fact]
    public void Calculate_NoOverrides_MatchesAnEmptyOverride()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 100_000m, accountBalances: [100_000m]),
        ]);

        // Act
        var without = _engine.Calculate(plan, Today);
        var withEmpty = _engine.Calculate(plan, Today, new ProjectionOverrides());

        // Assert
        Assert.Equal(without.Summary.BalanceAtRetirement, withEmpty.Summary.BalanceAtRetirement);
    }
}
