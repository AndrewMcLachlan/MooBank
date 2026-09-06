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
        var firstYear = _engine.CalculateWithoutPension(plan, Today).Years.ElementAt(1);

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
        var firstYear = _engine.CalculateWithoutPension(plan, Today).Years.ElementAt(1);

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
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

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
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

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
    [InlineData(GrowthStrategy.Conservative, 0.049)]
    [InlineData(GrowthStrategy.Balanced, 0.061)]
    [InlineData(GrowthStrategy.Growth, 0.064)]
    [InlineData(GrowthStrategy.HighGrowth, 0.068)]
    [InlineData(GrowthStrategy.Moderate, 0.057)]
    public void For_NamedStrategy_UsesTheRecordedRate(GrowthStrategy strategy, decimal expected)
    {
        // Arrange
        var rates = TestEntities.StrategyRates(TestEntities.CreatePlan());

        // Act: a custom rate is offered and should be ignored, because the strategy is not Custom.
        var rate = rates.For(strategy, customRate: 0.99m);

        // Assert
        Assert.Equal(expected, rate);
    }

    /// <summary>
    /// Given the custom growth strategy
    /// When its return rate is resolved
    /// Then the member's own rate should be used
    /// </summary>
    [Fact]
    public void For_Custom_UsesTheMembersOwnRate()
    {
        // Arrange
        var rates = TestEntities.StrategyRates(TestEntities.CreatePlan());

        // Act
        var rate = rates.For(GrowthStrategy.Custom, customRate: 0.0625m);

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
        var projection = _engine.CalculateWithoutPension(plan, Today);

        // Assert
        // 4.9% on 100,000 plus 6.8% on 100,000.
        Assert.Equal(11_700m, projection.Years.ElementAt(1).InvestmentReturn);

        var cautious = projection.Members.Single(m => m.Name == "Cautious");
        var bold = projection.Members.Single(m => m.Name == "Bold");
        Assert.Equal(0.049m, cautious.ReturnRate);
        Assert.Equal(0.068m, bold.ReturnRate);
        Assert.True(bold.BalanceAtRetirement > cautious.BalanceAtRetirement);
    }

    /// <summary>
    /// Given a member who moves to a different strategy at retirement
    /// When the projection runs
    /// Then their balance should earn the accumulation rate first and the retirement rate after
    /// </summary>
    /// <remarks>
    /// A portfolio built for twenty years of accumulation is not the one most people draw an income
    /// from, and the difference compounds either side of the same date.
    /// </remarks>
    [Fact]
    public void Calculate_AMemberWhoDeRisksAtRetirement_ChangesRateAtTheirRetirementYear()
    {
        // Arrange: Growth at 6.4% until 65, Conservative at 4.9% from then on.
        var plan = TestEntities.CreatePlan(inflationRate: 0m, superGuaranteeRate: 0m, members: [
            TestEntities.CreateMember(currentAge: 63, retirementAge: 65, currentIncome: 0m,
                growthStrategy: GrowthStrategy.Growth, retirementGrowthStrategy: GrowthStrategy.Conservative,
                accountBalances: [100_000m]),
        ]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert: the last accumulating year earns 6.4%, the first retired year 4.9%.
        Assert.Equal(6_400m, years[1].InvestmentReturn);
        var atRetirement = years[2].OpeningBalance;
        Assert.Equal(Math.Round(atRetirement * 0.049m, 2), years[2].InvestmentReturn);
    }

    /// <summary>
    /// Given a member whose pay does not grow
    /// When the projection runs
    /// Then their contribution should be the same figure every year
    /// </summary>
    /// <remarks>
    /// Salary growth follows the plan's inflation unless the member says otherwise, which is an
    /// assumption worth being able to contradict: pay rises are not automatic.
    /// </remarks>
    [Fact]
    public void Calculate_AMemberWithNoSalaryGrowth_ContributesTheSameEachYear()
    {
        // Arrange: inflation is 5%, but this member's pay is flat.
        var plan = TestEntities.CreatePlan(inflationRate: 0.05m, superGuaranteeRate: 0.10m, contributionsTaxRate: 0m, members: [
            TestEntities.CreateMember(currentAge: 40, retirementAge: 65, currentIncome: 100_000m,
                salaryGrowthRate: 0m, accountBalances: [10_000m]),
        ]);

        // Act
        var years = _engine.CalculateWithoutPension(plan, Today).Years.ToList();

        // Assert
        Assert.Equal(10_000m, years[1].Contributions);
        Assert.Equal(10_000m, years[3].Contributions);
        Assert.Equal(10_000m, years[10].Contributions);
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
        var member = _engine.CalculateWithoutPension(plan, Today).Members.Single();

        // Assert
        Assert.Equal(GrowthStrategy.Growth, member.GrowthStrategy);
        Assert.Equal(0.064m, member.ReturnRate);
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
        var projection = _engine.CalculateWithoutPension(plan, Today, overrides);

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
        var firstYear = _engine.CalculateWithoutPension(plan, Today, overrides).Years.ElementAt(1);

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
        var member = TestEntities.CreateMember(currentAge: 60, retirementAge: 65, currentIncome: 0m, accountBalances: [100_000m]);
        var plan = TestEntities.CreatePlan(members: [member]);

        // The slider sets a rate and the strategy together: a rate on its own belongs to nobody.
        var overrides = new ProjectionOverrides
        {
            Members = [new MemberOverride { MemberId = member.Id, GrowthStrategy = GrowthStrategy.Custom, CustomReturnRate = 0.20m }],
        };

        // Act
        var firstYear = _engine.CalculateWithoutPension(plan, Today, overrides).Years.ElementAt(1);

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
            InflationRate = 0.09m,
            LifeExpectancy = 100,
            Members = [new MemberOverride { MemberId = member.Id, RetirementAge = 70, CurrentIncome = 999_999m, SalarySacrifice = 50_000m, CurrentAge = 30, GrowthStrategy = GrowthStrategy.HighGrowth }],
        };

        // Act
        _engine.CalculateWithoutPension(plan, Today, overrides);

        // Assert
        Assert.Equal(0.10m, member.CustomReturnRate);
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
        var projection = _engine.CalculateWithoutPension(plan, Today, overrides);

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
        var without = _engine.CalculateWithoutPension(plan, Today);
        var withEmpty = _engine.CalculateWithoutPension(plan, Today, new ProjectionOverrides());

        // Assert
        Assert.Equal(without.Summary.BalanceAtRetirement, withEmpty.Summary.BalanceAtRetirement);
    }

    /// <summary>
    /// Given two members on different strategies
    /// When the summary is produced
    /// Then the headline real return should be weighted by what each of them holds
    /// </summary>
    /// <remarks>
    /// A household of a Conservative member and a High Growth one earns neither rate. Reporting
    /// either would describe a portfolio nobody holds, and would not agree with the projection
    /// printed beside it.
    /// </remarks>
    [Fact]
    public void Calculate_MembersOnDifferentStrategies_ReportsTheWeightedRealReturn()
    {
        // Arrange: three times as much on the conservative side, so a plain average would differ.
        var plan = TestEntities.CreatePlan(inflationRate: 0m, members: [
            TestEntities.CreateMember(currentAge: 60, currentIncome: 0m, growthStrategy: GrowthStrategy.Conservative, accountBalances: [300_000m]),
            TestEntities.CreateMember(currentAge: 60, currentIncome: 0m, growthStrategy: GrowthStrategy.HighGrowth, accountBalances: [100_000m]),
        ]);

        // Act
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert: (300,000 x 4.9% + 100,000 x 6.8%) / 400,000, and no inflation to discount.
        Assert.Equal(0.05375m, summary.RealReturnRate, 5);
    }

    /// <summary>
    /// Given a member on a named strategy
    /// When the summary is produced
    /// Then the household return should be that strategy's, not a figure from the plan
    /// </summary>
    /// <remarks>
    /// The plan used to carry a rate of its own that the summary reported whether or not anyone was
    /// invested at it. A single-member household is the sharpest case: the headline is simply that
    /// member's rate.
    /// </remarks>
    [Fact]
    public void Calculate_OneNamedMember_ReportsThatStrategysRealReturn()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(inflationRate: 0m, members: [
            TestEntities.CreateMember(currentAge: 60, currentIncome: 0m, growthStrategy: GrowthStrategy.Growth, accountBalances: [100_000m]),
        ]);

        // Act
        var summary = _engine.CalculateWithoutPension(plan, Today).Summary;

        // Assert
        Assert.Equal(0.064m, summary.RealReturnRate, 5);
    }
}
