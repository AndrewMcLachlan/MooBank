#nullable enable
using System.Text.Json;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Services;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

[Trait("Category", "Unit")]
public class ForecastEngineTests
{
    private readonly TestMocks _mocks;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ForecastEngineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Calculate_SimplePlan_ReturnsForecastResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(plan.Id, result.PlanId);
        Assert.Equal(3, result.Months.Count()); // Jan, Feb, Mar
    }

    [Fact]
    public async Task Calculate_ManualStartingBalance_UsesProvidedAmount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 15000m,
            startingBalanceMode: StartingBalanceMode.ManualAmount,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(15000m, firstMonth.OpeningBalance);
    }

    [Fact]
    public async Task Calculate_CalculatedStartingBalance_CalculatesFromAccounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            endDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
            startingBalance: null,
            startingBalanceMode: StartingBalanceMode.CalculatedCurrent,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            Balance = 20000m,
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        SetupEmptyReportMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(20000m, firstMonth.OpeningBalance);
    }

    [Fact]
    public async Task Calculate_WithBaselineOutgoings_IncludesInMonthlyCalculation()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 3);

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            Balance = 10000m,
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Setup historical outgoings - 3000 per month over 3 months
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = new List<CreditDebitTotal>
                {
                    new() { TransactionType = TransactionFilterType.Debit, Total = 9000m },
                }
            });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(3000m, firstMonth.BaselineOutgoingsTotal); // 9000 / 3 months
    }

    [Fact]
    public async Task Calculate_WithPlannedExpense_SubtractsFromBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0); // No baseline

        // Add a planned expense
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Car Insurance",
            ItemType = PlannedItemType.Expense,
            Amount = 1200m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate
            {
                FixedDate = new DateOnly(2024, 1, 15)
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(-1200m, firstMonth.PlannedItemsTotal);
    }

    [Fact]
    public async Task Calculate_WithPlannedIncome_AddsToBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 10000m,
            monthlyIncome: 0m, // No regular income
            lookbackMonths: 0); // No baseline

        // Add a planned income
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Tax Refund",
            ItemType = PlannedItemType.Income,
            Amount = 2000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate
            {
                FixedDate = new DateOnly(2024, 1, 20)
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(2000m, firstMonth.PlannedItemsTotal);
    }

    [Fact]
    public async Task Calculate_ScheduledItem_ExpandsToMultipleMonths()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        // Add a monthly scheduled expense
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Monthly Subscription",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule
            {
                Frequency = ScheduleFrequency.Monthly,
                AnchorDate = new DateOnly(2024, 1, 1),
                Interval = 1,
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Months.Count());
        Assert.All(result.Months, m => Assert.Equal(-100m, m.PlannedItemsTotal));
    }

    [Fact]
    public async Task Calculate_FlexibleWindowEvenlySpread_DistributesAcrossMonths()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        // Add a flexible window expense spread over 3 months
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Vacation Fund",
            ItemType = PlannedItemType.Expense,
            Amount = 3000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FlexibleWindow,
            FlexibleWindow = new PlannedItemFlexibleWindow
            {
                StartDate = new DateOnly(2024, 1, 1),
                EndDate = new DateOnly(2024, 3, 31),
                AllocationMode = AllocationMode.EvenlySpread,
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Months.Count());
        Assert.All(result.Months, m => Assert.Equal(-1000m, m.PlannedItemsTotal)); // 3000 / 3 months
    }

    [Fact]
    public async Task Calculate_FlexibleWindowAllAtEnd_AllocatesToLastMonth()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        // Add a flexible window expense all at end
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Year-End Bonus Spend",
            ItemType = PlannedItemType.Expense,
            Amount = 3000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FlexibleWindow,
            FlexibleWindow = new PlannedItemFlexibleWindow
            {
                StartDate = new DateOnly(2024, 1, 1),
                EndDate = new DateOnly(2024, 3, 31),
                AllocationMode = AllocationMode.AllAtEnd,
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(0m, months[0].PlannedItemsTotal);
        Assert.Equal(0m, months[1].PlannedItemsTotal);
        Assert.Equal(-3000m, months[2].PlannedItemsTotal);
    }

    [Fact]
    public async Task Calculate_ExcludedItem_NotIncludedInCalculation()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        // Add an excluded planned expense
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Excluded Expense",
            ItemType = PlannedItemType.Expense,
            Amount = 1000m,
            IsIncluded = false, // Not included
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate
            {
                FixedDate = new DateOnly(2024, 1, 15)
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(0m, firstMonth.PlannedItemsTotal);
    }

    [Fact]
    public async Task Calculate_SummaryLowestBalance_IdentifiesCorrectMonth()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 1000m,
            lookbackMonths: 0);

        // Add a big expense in month 2
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Big Expense",
            ItemType = PlannedItemType.Expense,
            Amount = 8000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate
            {
                FixedDate = new DateOnly(2024, 2, 15)
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new DateOnly(2024, 2, 1), result.Summary.LowestBalanceMonth);
    }

    [Fact]
    public async Task Calculate_MonthsBelowZero_CountsCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 1000m,
            monthlyIncome: 500m,
            lookbackMonths: 0);

        // Add expenses that will drive balance negative
        var plannedItem = new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Big Monthly Expense",
            ItemType = PlannedItemType.Expense,
            Amount = 2000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule
            {
                Frequency = ScheduleFrequency.Monthly,
                AnchorDate = new DateOnly(2024, 1, 1),
                Interval = 1,
            }
        };
        plan.PlannedItems.Add(plannedItem);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        // Month 1: 1000 + 500 - 2000 = -500 (below zero)
        // Month 2: -500 + 500 - 2000 = -2000 (below zero)
        // Month 3: -2000 + 500 - 2000 = -3500 (below zero)
        Assert.Equal(3, result.Summary.MonthsBelowZero);
    }

    [Fact]
    public async Task Calculate_TotalIncomeAndOutgoings_SumsCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(15000m, result.Summary.TotalIncome); // 5000 * 3 months
    }

    [Fact]
    public async Task Calculate_EmptyPlan_ReturnsEmptyMonths()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        // Create a plan with start date after end date (invalid range)
        var plan = new DomainForecastPlan(Guid.NewGuid())
        {
            FamilyId = _mocks.User.FamilyId,
            Name = "Invalid Plan",
            StartDate = new DateOnly(2024, 2, 1),
            EndDate = new DateOnly(2024, 1, 31), // Before start
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            AccountScopeMode = AccountScopeMode.AllAccounts,
        };

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Months);
    }

    [Fact]
    public async Task Calculate_SelectedAccounts_UsesOnlySelectedAccounts()
    {
        // Arrange
        var selectedAccountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [selectedAccountId, otherAccountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 1, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0,
            accountScopeMode: AccountScopeMode.SelectedAccounts);

        plan.SetAccounts([selectedAccountId]);

        var mockAccount = new LogicalAccount(selectedAccountId, [])
        {
            Name = "Selected Account",
            Balance = 5000m,
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == selectedAccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        SetupEmptyReportMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        _mocks.InstrumentRepositoryMock.Verify(
            r => r.Get(It.Is<IEnumerable<Guid>>(ids => ids.Count() == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Calculate_SavingsAccountsExcludedFromHistoricalAnalysis()
    {
        // Arrange
        var transactionAccountId = Guid.NewGuid();
        var savingsAccountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [transactionAccountId, savingsAccountId]));

        var plan = CreatePlanWithStrategies(
            startDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            endDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
            startingBalance: null,
            startingBalanceMode: StartingBalanceMode.CalculatedCurrent,
            monthlyIncome: 5000m,
            lookbackMonths: 3);

        var transactionAccount = new LogicalAccount(transactionAccountId, [])
        {
            Name = "Transaction Account",
            Balance = 10000m,
            AccountType = AccountType.Transaction,
        };

        var savingsAccount = new LogicalAccount(savingsAccountId, [])
        {
            Name = "Savings Account",
            Balance = 50000m,
            AccountType = AccountType.Savings,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { transactionAccount, savingsAccount });

        // Historical analysis should only be called for non-savings accounts
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == transactionAccountId),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        // Starting balance should include both accounts
        var firstMonth = result.Months.First();
        Assert.Equal(60000m, firstMonth.OpeningBalance); // 10000 + 50000
    }

    private DomainForecastPlan CreatePlanWithStrategies(
        Guid? id = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal? startingBalance = null,
        StartingBalanceMode startingBalanceMode = StartingBalanceMode.ManualAmount,
        decimal monthlyIncome = 0m,
        int lookbackMonths = 12,
        AccountScopeMode accountScopeMode = AccountScopeMode.AllAccounts)
    {
        var incomeStrategy = new IncomeStrategy
        {
            Mode = "ManualRecurring",
            ManualRecurring = new ManualRecurringIncome { Amount = monthlyIncome, Frequency = "Monthly" }
        };

        var outgoingStrategy = new OutgoingStrategy
        {
            Mode = "HistoricalAverage",
            LookbackMonths = lookbackMonths
        };

        return new DomainForecastPlan(id ?? Guid.NewGuid())
        {
            FamilyId = _mocks.User.FamilyId,
            Name = "Test Plan",
            StartDate = startDate ?? new DateOnly(2024, 1, 1),
            EndDate = endDate ?? new DateOnly(2024, 12, 31),
            StartingBalanceMode = startingBalanceMode,
            StartingBalanceAmount = startingBalance,
            AccountScopeMode = accountScopeMode,
            CurrencyCode = "AUD",
            IncomeStrategySerialized = JsonSerializer.Serialize(incomeStrategy, JsonOptions),
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Given actual balance data exists for consecutive past months
    /// When the forecast is calculated
    /// Then the baseline outgoings should be recalculated from actual spending
    /// and the projected line should remain a consistent chain from starting balance
    /// </summary>
    [Fact]
    public async Task Calculate_WithActualBalances_RecalculatesBaselineOutgoings()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        // Plan: 3 months, income 5000/month, no historical baseline
        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        // Actual balances for Jan and Feb:
        // Dec closing = 10000 (opening for Jan), Jan closing = 12000 (opening for Feb)
        // Actual outgoings for Jan: 10000 + 5000 + 0 - 12000 = 3000
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>
            {
                [accountId] =
                [
                    new MonthlyBalance { PeriodEnd = new DateOnly(2023, 12, 31), Balance = 10000m },
                    new MonthlyBalance { PeriodEnd = new DateOnly(2024, 1, 31), Balance = 12000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // Baseline should be recalculated to 3000 (derived from actual balance change)
        Assert.All(months, m => Assert.Equal(3000m, m.BaselineOutgoingsTotal));

        // Projected line chains from starting balance with updated baseline:
        // Jan: 10000 + 5000 - 3000 = 12000
        // Feb: 12000 + 5000 - 3000 = 14000
        // Mar: 14000 + 5000 - 3000 = 16000
        Assert.Equal(10000m, months[0].OpeningBalance);
        Assert.Equal(12000m, months[0].ClosingBalance);
        Assert.Equal(12000m, months[1].OpeningBalance);
        Assert.Equal(14000m, months[1].ClosingBalance);
        Assert.Equal(14000m, months[2].OpeningBalance);
        Assert.Equal(16000m, months[2].ClosingBalance);

        // Summary also reflects updated baseline
        Assert.Equal(3000m, result.Summary.MonthlyBaselineOutgoings);
    }

    /// <summary>
    /// Given actual balance data exists for multiple consecutive months
    /// When the forecast is calculated
    /// Then the baseline should be the average of actual outgoings across those months
    /// </summary>
    [Fact]
    public async Task Calculate_MultipleActualMonths_AveragesActualOutgoings()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 4, 30),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        // Actual balances for Jan, Feb, Mar:
        // Dec closing = 10000, Jan closing = 13000, Feb closing = 14000
        // Jan actual outgoings: 10000 + 5000 - 13000 = 2000
        // Feb actual outgoings: 13000 + 5000 - 14000 = 4000
        // Average = (2000 + 4000) / 2 = 3000
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>
            {
                [accountId] =
                [
                    new MonthlyBalance { PeriodEnd = new DateOnly(2023, 12, 31), Balance = 10000m },
                    new MonthlyBalance { PeriodEnd = new DateOnly(2024, 1, 31), Balance = 13000m },
                    new MonthlyBalance { PeriodEnd = new DateOnly(2024, 2, 29), Balance = 14000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // Baseline should be average of actual outgoings: (2000 + 4000) / 2 = 3000
        Assert.All(months, m => Assert.Equal(3000m, m.BaselineOutgoingsTotal));
    }

    /// <summary>
    /// Given no actual balance data exists
    /// When the forecast is calculated
    /// Then the historical baseline should be used (existing behavior preserved)
    /// </summary>
    [Fact]
    public async Task Calculate_NoActualBalances_UsesHistoricalBaseline()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // All months should use predicted chain from starting balance with 0 baseline
        Assert.Equal(10000m, months[0].OpeningBalance);
        Assert.Equal(15000m, months[0].ClosingBalance); // 10000 + 5000
        Assert.Equal(15000m, months[1].OpeningBalance);
        Assert.Equal(20000m, months[1].ClosingBalance); // 15000 + 5000
        Assert.Equal(20000m, months[2].OpeningBalance);
        Assert.Equal(25000m, months[2].ClosingBalance); // 20000 + 5000

        // No actual balances
        Assert.All(months, m => Assert.Null(m.ActualBalance));
    }

    /// <summary>
    /// Given actual balance grows faster than income + planned can explain (e.g. unexpected deposit)
    /// When the forecast is calculated
    /// Then the anomalous month should be skipped and the historical baseline used as fallback
    /// </summary>
    [Fact]
    public async Task Calculate_NegativeDerivedOutgoings_SkipsAnomalousMonth()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 3, 31),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0); // fallback baseline = 0

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        // Actual balances: Dec closing = 10000, Jan closing = 20000
        // Derived outgoings for Jan: 10000 + 5000 + 0 - 20000 = -5000 (negative - anomalous)
        // Should be skipped, falling back to historical baseline (0)
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>
            {
                [accountId] =
                [
                    new MonthlyBalance { PeriodEnd = new DateOnly(2023, 12, 31), Balance = 10000m },
                    new MonthlyBalance { PeriodEnd = new DateOnly(2024, 1, 31), Balance = 20000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // Anomalous month skipped, so baseline falls back to 0 (historical)
        Assert.All(months, m => Assert.Equal(0m, m.BaselineOutgoingsTotal));

        // Projected line: 10000 + 5000 = 15000, 15000 + 5000 = 20000, etc.
        Assert.Equal(15000m, months[0].ClosingBalance);
        Assert.Equal(20000m, months[1].ClosingBalance);
        Assert.Equal(25000m, months[2].ClosingBalance);
    }

    private void SetupEmptyRepositoryMocks()
    {
        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        SetupEmptyReportMocks();
    }

    private void SetupEmptyReportMocks()
    {
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());
    }
}
