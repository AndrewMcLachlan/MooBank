#nullable enable
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the retirement projection engine.
/// </summary>
/// <remarks>
/// The default test plan uses a 10% return, no inflation, a 10% employer contribution and no
/// contributions tax, so every figure below can be checked by hand.
/// </remarks>
[Trait("Category", "Unit")]
public class RetirementProjectionEngineTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// Given a single member five years from retirement
    /// When the projection is run
    /// Then there should be a row for the starting position plus one per year to retirement
    /// </summary>
    [Fact]
    public void Calculate_SingleMember_ProducesAYearPerYearToRetirement()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(retirementAge: 65, accountBalances: [100_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.Equal(6, projection.Years.Count());
        Assert.Equal(2026, projection.Years.First().Year);
        Assert.Equal(2031, projection.Years.Last().Year);
    }

    /// <summary>
    /// Given a starting balance of 100,000, income of 100,000, a 10% return and a 10% contribution
    /// When the projection is run
    /// Then the first year should add a 10,000 return and a 10,000 contribution
    /// </summary>
    /// <remarks>
    /// The return applies to the opening balance only: that year's contributions earn nothing until
    /// the following year.
    /// </remarks>
    [Fact]
    public void Calculate_FirstYear_AppliesReturnToOpeningBalanceOnly()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(retirementAge: 65, currentIncome: 100_000m, accountBalances: [100_000m]),
        ]);

        // Act
        var firstProjectedYear = _engine.Calculate(plan, Today).Years.ElementAt(1);

        // Assert
        Assert.Equal(100_000m, firstProjectedYear.OpeningBalance);
        Assert.Equal(10_000m, firstProjectedYear.InvestmentReturn);
        Assert.Equal(10_000m, firstProjectedYear.Contributions);
        Assert.Equal(120_000m, firstProjectedYear.ClosingBalance);
    }

    /// <summary>
    /// Given a five year projection
    /// When it is run
    /// Then each year should compound on the last
    /// </summary>
    [Theory]
    [InlineData(1, 100_000, 120_000)]
    [InlineData(2, 120_000, 142_000)]
    [InlineData(3, 142_000, 166_200)]
    [InlineData(4, 166_200, 192_820)]
    [InlineData(5, 192_820, 222_102)]
    public void Calculate_EachYear_CompoundsOnTheLast(int yearOffset, decimal expectedOpening, decimal expectedClosing)
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(retirementAge: 65, currentIncome: 100_000m, accountBalances: [100_000m]),
        ]);

        // Act
        var year = _engine.Calculate(plan, Today).Years.ElementAt(yearOffset);

        // Assert
        Assert.Equal(expectedOpening, year.OpeningBalance);
        Assert.Equal(expectedClosing, year.ClosingBalance);
    }

    /// <summary>
    /// Given a member with more than one superannuation account
    /// When the projection is run
    /// Then their starting balance should be the sum of those accounts
    /// </summary>
    [Fact]
    public void Calculate_MemberWithSeveralAccounts_SumsTheBalances()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(accountBalances: [50_000m, 30_000m, 20_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.Equal(100_000m, projection.Summary.CurrentBalance);
        Assert.Equal(100_000m, projection.Members.Single().CurrentBalance);
    }

    /// <summary>
    /// Given inflation of 2% and contributions tax of 15%
    /// When the projection is run
    /// Then contributions should be reduced by the tax and indexed from the second year
    /// </summary>
    [Fact]
    public void Calculate_WithInflationAndContributionsTax_IndexesIncomeAndTaxesContributions()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0.02m,
            contributionsTaxRate: 0.15m,
            members: [TestEntities.CreateMember(retirementAge: 65, currentIncome: 100_000m, accountBalances: [0m])]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        // 100,000 * 10% * (1 - 15%)
        Assert.Equal(8_500m, years[1].Contributions);
        // Income indexed by one year of inflation: 102,000 * 10% * 85%
        Assert.Equal(8_670m, years[2].Contributions);
    }

    /// <summary>
    /// Given a plan with inflation
    /// When the projection is run
    /// Then the closing balance in today's dollars should be below the nominal balance
    /// </summary>
    [Fact]
    public void Calculate_WithInflation_DiscountsBalanceToTodaysDollars()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0.03m,
            members: [TestEntities.CreateMember(retirementAge: 65, accountBalances: [100_000m])]);

        // Act
        var finalYear = _engine.Calculate(plan, Today).Years.Last();

        // Assert
        Assert.True(finalYear.ClosingBalanceInTodaysDollars < finalYear.ClosingBalance);
        // Five years of 3% inflation.
        var expected = Math.Round(finalYear.ClosingBalance / (decimal)Math.Pow(1.03d, 5), 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expected, finalYear.ClosingBalanceInTodaysDollars, 2);
    }

    /// <summary>
    /// Given a plan with no inflation
    /// When the projection is run
    /// Then today's dollars should equal the nominal balance
    /// </summary>
    [Fact]
    public void Calculate_WithoutInflation_TodaysDollarsMatchesNominal()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(accountBalances: [100_000m]),
        ]);

        // Act
        var finalYear = _engine.Calculate(plan, Today).Years.Last();

        // Assert
        Assert.Equal(finalYear.ClosingBalance, finalYear.ClosingBalanceInTodaysDollars);
    }

    /// <summary>
    /// Given a member who has already passed their retirement age
    /// When the projection is run
    /// Then they should be reported as retired with no years left to run
    /// </summary>
    [Fact]
    public void Calculate_MemberPastRetirementAge_IsReportedAsAlreadyRetired()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(retirementAge: 55, accountBalances: [100_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);
        var member = projection.Members.Single();

        // Assert
        Assert.True(member.AlreadyRetired);
        Assert.Equal(0, member.YearsToRetirement);
        Assert.Equal(100_000m, member.BalanceAtRetirement);
        Assert.Single(projection.Years);
        Assert.True(projection.Years.Single().AllRetired);
    }

    /// <summary>
    /// Given two members retiring in different years
    /// When the projection is run
    /// Then it should run to the later retirement and each member's balance should be captured at
    /// their own retirement
    /// </summary>
    [Fact]
    public void Calculate_MembersRetiringInDifferentYears_CapturesEachAtTheirOwnRetirement()
    {
        // Arrange
        var earlier = TestEntities.CreateMember(name: "Early", retirementAge: 62, currentIncome: 100_000m, accountBalances: [100_000m]);
        var later = TestEntities.CreateMember(name: "Late", retirementAge: 67, currentIncome: 100_000m, accountBalances: [100_000m]);
        var plan = TestEntities.CreatePlan(members: [earlier, later]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.Equal(2033, projection.Summary.RetirementYear);

        var early = projection.Members.Single(m => m.Name == "Early");
        var late = projection.Members.Single(m => m.Name == "Late");

        Assert.Equal(2028, early.RetirementYear);
        Assert.Equal(2033, late.RetirementYear);
        // Two years of the same starting position: 100,000 -> 120,000 -> 142,000.
        Assert.Equal(142_000m, early.BalanceAtRetirement);
        Assert.True(late.BalanceAtRetirement > early.BalanceAtRetirement);
    }

    /// <summary>
    /// Given a member who has retired while another is still working
    /// When the projection reaches a year after the first has retired
    /// Then only the still-working member should contribute
    /// </summary>
    [Fact]
    public void Calculate_AfterAMemberRetires_OnlyTheOthersContribute()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "Early", retirementAge: 61, currentIncome: 100_000m, accountBalances: [0m]),
            TestEntities.CreateMember(name: "Late", retirementAge: 65, currentIncome: 100_000m, accountBalances: [0m]),
        ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        // Both contribute in the first year, only the later member from the second onwards.
        Assert.Equal(20_000m, years[1].Contributions);
        Assert.Equal(10_000m, years[2].Contributions);
    }

    /// <summary>
    /// Given a member who has retired
    /// When the projection continues for a still-working member
    /// Then the retired member's balance should keep earning returns
    /// </summary>
    [Fact]
    public void Calculate_AfterAMemberRetires_TheirBalanceStillEarnsReturns()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "Early", retirementAge: 61, currentIncome: 0m, accountBalances: [100_000m]),
            TestEntities.CreateMember(name: "Late", retirementAge: 65, currentIncome: 0m, accountBalances: [0m]),
        ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        // The retired member's 110,000 earns 11,000 in the second year.
        Assert.Equal(11_000m, years[2].InvestmentReturn);
    }

    /// <summary>
    /// Given a plan with no members
    /// When the projection is run
    /// Then it should return an empty projection rather than fail
    /// </summary>
    [Fact]
    public void Calculate_NoMembers_ReturnsAnEmptyProjection()
    {
        // Arrange
        var plan = TestEntities.CreatePlan();

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.Empty(projection.Years);
        Assert.Empty(projection.Members);
        Assert.Equal(0m, projection.Summary.BalanceAtRetirement);
        Assert.Equal(2026, projection.Summary.RetirementYear);
    }

    /// <summary>
    /// Given a member with a birthday later in the year
    /// When the projection is run
    /// Then their age should not count the birthday they have not reached
    /// </summary>
    [Fact]
    public void Calculate_BirthdayNotYetReached_DoesNotCountTheComingYear()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 59, retirementAge: 65, accountBalances: [0m]),
        ]);

        // Act
        var member = _engine.Calculate(plan, Today).Members.Single();

        // Assert
        Assert.Equal(59, member.CurrentAge);
        Assert.Equal(6, member.YearsToRetirement);
    }

    /// <summary>
    /// Given a nominal return and an inflation rate
    /// When the real return is derived
    /// Then it should use the Fisher relation rather than subtracting the rates
    /// </summary>
    [Fact]
    public void RealReturnRate_UsesTheFisherRelation()
    {
        // Act
        var real = RetirementProjectionEngine.RealReturnRate(0.10m, 0.02m);

        // Assert
        // 1.10 / 1.02 - 1 is 7.84%, not the 8% that subtracting the rates would give.
        Assert.Equal(0.0784m, real, 4);
        Assert.NotEqual(0.08m, Math.Round(real, 4));
    }

    /// <summary>
    /// Given a balance and no real return
    /// When the annual drawdown is calculated
    /// Then the balance should be spread evenly across the years
    /// </summary>
    [Fact]
    public void AnnualDrawdown_WithNoRealReturn_SpreadsTheBalanceEvenly()
    {
        // Act
        var drawdown = RetirementProjectionEngine.AnnualDrawdown(100_000m, 0m, 10);

        // Assert
        Assert.Equal(10_000m, drawdown);
    }

    /// <summary>
    /// Given a balance earning a real return during drawdown
    /// When the annual drawdown is calculated
    /// Then it should exceed an even split, because the remaining balance keeps earning
    /// </summary>
    [Fact]
    public void AnnualDrawdown_WithARealReturn_ExceedsAnEvenSplit()
    {
        // Act
        var drawdown = RetirementProjectionEngine.AnnualDrawdown(100_000m, 0.05m, 10);

        // Assert
        Assert.True(drawdown > 10_000m);
        // The standard annuity payment for 100,000 over 10 years at 5%.
        Assert.Equal(12_950.46m, drawdown, 2);
    }

    /// <summary>
    /// Given a drawdown horizon of zero years or a balance of nothing
    /// When the annual drawdown is calculated
    /// Then it should be nothing rather than dividing by zero
    /// </summary>
    [Theory]
    [InlineData(100_000, 0)]
    [InlineData(0, 10)]
    [InlineData(-100, 10)]
    public void AnnualDrawdown_WithNothingToDrawOn_IsZero(decimal balance, int years)
    {
        // Act
        var drawdown = RetirementProjectionEngine.AnnualDrawdown(balance, 0.05m, years);

        // Assert
        Assert.Equal(0m, drawdown);
    }

    /// <summary>
    /// Given a plan whose life expectancy is beyond the retirement age
    /// When the projection is run
    /// Then a member should be given an annual retirement income
    /// </summary>
    [Fact]
    public void Calculate_LifeExpectancyBeyondRetirement_ProducesRetirementIncome()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            lifeExpectancy: 90,
            members: [TestEntities.CreateMember(retirementAge: 65, accountBalances: [100_000m])]);

        // Act
        var projection = _engine.Calculate(plan, Today);
        var member = projection.Members.Single();

        // Assert
        Assert.True(member.AnnualRetirementIncomeInTodaysDollars > 0m);
        Assert.Equal(member.AnnualRetirementIncomeInTodaysDollars, projection.Summary.AnnualRetirementIncomeInTodaysDollars);
    }

    /// <summary>
    /// Given a life expectancy at or below the retirement age
    /// When the projection is run
    /// Then there should be no drawdown period and so no retirement income
    /// </summary>
    [Fact]
    public void Calculate_LifeExpectancyAtRetirementAge_ProducesNoRetirementIncome()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            lifeExpectancy: 65,
            members: [TestEntities.CreateMember(retirementAge: 65, accountBalances: [100_000m])]);

        // Act
        var member = _engine.Calculate(plan, Today).Members.Single();

        // Assert
        Assert.Equal(0m, member.AnnualRetirementIncomeInTodaysDollars);
    }

    /// <summary>
    /// Given a household of two members
    /// When the projection is run
    /// Then the summary income should be the sum of the members' incomes
    /// </summary>
    [Fact]
    public void Calculate_Household_SummaryIncomeIsTheSumOfMembers()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(name: "One", retirementAge: 65, accountBalances: [100_000m]),
            TestEntities.CreateMember(name: "Two", retirementAge: 65, accountBalances: [200_000m]),
        ]);

        // Act
        var projection = _engine.Calculate(plan, Today);

        // Assert
        Assert.Equal(
            projection.Members.Sum(m => m.AnnualRetirementIncomeInTodaysDollars),
            projection.Summary.AnnualRetirementIncomeInTodaysDollars);
        Assert.Equal(300_000m, projection.Summary.CurrentBalance);
    }

    /// <summary>
    /// Given any projection
    /// When each year is inspected
    /// Then the closing balance should reconcile with the opening balance and the year's movements
    /// </summary>
    [Fact]
    public void Calculate_EveryYear_Reconciles()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(
            inflationRate: 0.025m,
            contributionsTaxRate: 0.15m,
            members: [
                TestEntities.CreateMember(retirementAge: 67, currentIncome: 95_000m, annualFees: 372m, insurancePremium: 364m, accountBalances: [180_000m]),
                TestEntities.CreateMember(currentAge: 53, retirementAge: 65, currentIncome: 70_000m, annualFees: 250m, insurancePremium: 180m, accountBalances: [120_000m]),
            ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        Assert.All(years, year =>
            Assert.Equal(year.OpeningBalance + year.Contributions + year.InvestmentReturn - year.Costs, year.ClosingBalance));
    }

    /// <summary>
    /// Given consecutive years
    /// When they are compared
    /// Then each year's opening balance should be the previous year's closing balance
    /// </summary>
    [Fact]
    public void Calculate_ConsecutiveYears_ChainOpeningToClosing()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(retirementAge: 67, accountBalances: [180_000m]),
        ]);

        // Act
        var years = _engine.Calculate(plan, Today).Years.ToList();

        // Assert
        for (var i = 1; i < years.Count; i++)
        {
            Assert.Equal(years[i - 1].ClosingBalance, years[i].OpeningBalance);
        }
    }
}
