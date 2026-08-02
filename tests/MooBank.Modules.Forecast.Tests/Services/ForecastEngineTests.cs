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
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

[Trait("Category", "Unit")]
public class ForecastEngineTests
{
    private readonly TestMocks _mocks;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// The regression tests' historical data runs January–June 2024, so the accounts must report
    /// data through the last day of June: the training window closes on the last <em>complete</em>
    /// month, and a window anchored on today would exclude all of it.
    /// </summary>
    private static readonly DateOnly TrainingDataThrough = new(2024, 6, 30);

    public ForecastEngineTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// A transaction account whose data runs to <paramref name="dataThrough"/>.
    /// </summary>
    private static LogicalAccount HistoricalAccount(Guid id, DateOnly dataThrough, decimal balance = 0m, string name = "Test Account") =>
        new(id, [])
        {
            Name = name,
            Balance = balance,
            AccountType = AccountType.Transaction,
            LastTransaction = dataThrough,
        };

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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
        _mocks.ReportReaderMock
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

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(1200m, firstMonth.PlannedExpensesTotal);
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(2000m, firstMonth.IncomeTotal);
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Months.Count());
        Assert.All(result.Months, m => Assert.Equal(100m, m.PlannedExpensesTotal));
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Months.Count());
        Assert.All(result.Months, m => Assert.Equal(1000m, m.PlannedExpensesTotal)); // 3000 / 3 months
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(0m, months[0].PlannedExpensesTotal);
        Assert.Equal(0m, months[1].PlannedExpensesTotal);
        Assert.Equal(3000m, months[2].PlannedExpensesTotal);
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        Assert.Equal(0m, firstMonth.PlannedExpensesTotal);
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.Is<IEnumerable<Guid>>(ids => ids.Count() == 1 && ids.First() == transactionAccountId),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
        AccountScopeMode accountScopeMode = AccountScopeMode.AllAccounts,
        IncomeCorrelatedSettings? incomeCorrelatedSettings = null)
    {
        var outgoingStrategy = new OutgoingStrategy
        {
            LookbackMonths = lookbackMonths,
            IncomeCorrelated = incomeCorrelatedSettings,
        };

        var planId = id ?? Guid.NewGuid();
        var start = startDate ?? new DateOnly(2024, 1, 1);

        var plan = new DomainForecastPlan(planId)
        {
            FamilyId = _mocks.User.FamilyId,
            Name = "Test Plan",
            StartDate = start,
            EndDate = endDate ?? new DateOnly(2024, 12, 31),
            StartingBalanceMode = startingBalanceMode,
            StartingBalanceAmount = startingBalance,
            AccountScopeMode = accountScopeMode,
            CurrencyCode = "AUD",
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        if (monthlyIncome > 0m)
        {
            plan.PlannedItems.Add(MonthlyIncome(planId, monthlyIncome, start));
        }

        return plan;
    }

    /// <summary>
    /// A recurring monthly income item — how a plan models a salary now that there is no fixed
    /// income figure.
    /// </summary>
    private static DomainPlannedItem MonthlyIncome(Guid planId, decimal amount, DateOnly from, DateOnly? until = null, string name = "Income") =>
        new(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = name,
            ItemType = PlannedItemType.Income,
            Amount = amount,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule
            {
                Frequency = ScheduleFrequency.Monthly,
                AnchorDate = from,
                Interval = 1,
                EndDate = until,
            },
        };

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

        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        // Actual balances for Jan and Feb:
        // Dec closing = 10000 (opening for Jan), Jan closing = 12000 (opening for Feb)
        // Actual outgoings for Jan: 10000 + 5000 + 0 - 12000 = 3000
        _mocks.ReportReaderMock
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

        // Actual monthly credits used to derive outgoings from balance changes
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] =
                [
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 1, 1), TransactionType = TransactionFilterType.Credit, Total = 5000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // Baseline should be recalculated to 3000 (derived from actual balance change).
        //
        // Also guards the double-count that used to hide here: the plan's 5000 of income is a
        // planned income item now, and the derivation used to add planned income back on top of the
        // actual credits that already contained it — which would read this month as 8000.
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
        Assert.Equal(3000m, result.Summary.Expenses.AverageMonthly);
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

        _mocks.ReportReaderMock
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
        _mocks.ReportReaderMock
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

        // Actual monthly credits used to derive outgoings from balance changes
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] =
                [
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 1, 1), TransactionType = TransactionFilterType.Credit, Total = 5000m },
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 2, 1), TransactionType = TransactionFilterType.Credit, Total = 5000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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

        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        // Actual balances: Dec closing = 10000, Jan closing = 20000
        // Derived outgoings for Jan: 10000 + 5000 + 0 - 20000 = -5000 (negative - anomalous)
        // Should be skipped, falling back to historical baseline (0)
        _mocks.ReportReaderMock
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
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

    /// <summary>
    /// Given a month containing both planned income and a planned expense
    /// When the forecast is calculated
    /// Then the two are reported separately as positive amounts, and still net into PlannedItemsTotal
    /// </summary>
    [Fact]
    public async Task Calculate_PlannedIncomeAndExpenseInSameMonth_ReportsBothSeparately()
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

        plan.PlannedItems.Add(new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Tax Refund",
            ItemType = PlannedItemType.Income,
            Amount = 2000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = new DateOnly(2024, 1, 20) }
        });

        plan.PlannedItems.Add(new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "School Fees",
            ItemType = PlannedItemType.Expense,
            Amount = 1200m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = new DateOnly(2024, 1, 15) }
        });

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var firstMonth = result.Months.First();
        // Income is the salary plus the refund; expenses stay on their own series rather than
        // being netted against it, which would hide both.
        Assert.Equal(7000m, firstMonth.IncomeTotal);
        Assert.Equal(1200m, firstMonth.PlannedExpensesTotal);
        // The balance takes both: 10000 + 7000 - 1200
        Assert.Equal(15800m, firstMonth.ClosingBalance);
    }

    /// <summary>
    /// Given historical transaction data for past months
    /// When the forecast is calculated
    /// Then each historical month exposes actual income (credits) and outgoings (absolute debits)
    /// </summary>
    [Fact]
    public async Task Calculate_HistoricalMonths_PopulatesActualIncomeAndOutgoings()
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

        // A transaction account so it survives the historical-analysis filter (which excludes savings).
        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>
            {
                new LogicalAccount(accountId, []) { Name = "Test Account", Balance = 10000m, AccountType = AccountType.Transaction },
            });

        // Actual credits/debits for Jan and Feb (debit totals come back negative from the SP)
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] =
                [
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 1, 1), TransactionType = TransactionFilterType.Credit, Total = 5000m },
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 1, 1), TransactionType = TransactionFilterType.Debit, Total = -3000m },
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 2, 1), TransactionType = TransactionFilterType.Credit, Total = 6000m },
                    new MonthlyCreditDebitTotal { Month = new DateOnly(2024, 2, 1), TransactionType = TransactionFilterType.Debit, Total = -2000m },
                ]
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(5000m, months[0].ActualIncome);
        Assert.Equal(3000m, months[0].ActualOutgoings);
        Assert.Equal(6000m, months[1].ActualIncome);
        Assert.Equal(2000m, months[1].ActualOutgoings);
        // March is historical but has no data — zero, not null
        Assert.Equal(0m, months[2].ActualIncome);
        Assert.Equal(0m, months[2].ActualOutgoings);
    }

    /// <summary>
    /// Given a plan whose months are entirely in the future
    /// When the forecast is calculated
    /// Then actual income and outgoings are null (no actuals exist yet)
    /// </summary>
    [Fact]
    public async Task Calculate_FutureMonths_HaveNullActualIncomeAndOutgoings()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            endDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(2)),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 0);

        SetupEmptyRepositoryMocks();

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result.Months, m => Assert.Null(m.ActualIncome));
        Assert.All(result.Months, m => Assert.Null(m.ActualOutgoings));
    }

    #region Income-Correlated Regression Tests

    /// <summary>
    /// Given historical data with a strong linear relationship between income and expenses
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the outgoings should vary per month based on projected income
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedValidRegression_VariesOutgoingsPerMonth()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 20000m,
            monthlyIncome: 6000m,
            lookbackMonths: 6);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(accountId, TrainingDataThrough) });

        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Historical monthly data: strong positive correlation between income and expenses
        // Income: 5000, 6000, 7000, 8000, 9000, 10000
        // Expense: 3000, 3500, 4000, 4500, 5000, 5500
        // Regression: expense = 500 + 0.5 * income, R² = 1.0
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m), (8000m, 4500m), (9000m, 5000m), (10000m, 5500m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(3, months.Count);

        // Regression: expense = 500 + 0.5 x income. The plan starts after the training window, so it
        // models no income inside it and there is no shortfall to correct for — expenses are priced
        // at the income the plan actually says it will have: 500 + 0.5 x 6000 = 3500.
        //
        // This used to read 4250, because the offset was the gap to a flat annual salary
        // (7500 - 6000 = 1500) and so priced spending at 7500 while crediting only 6000. That is the
        // shape of the defect this change removes: spending at the high-income level, earning at the
        // low one, in every month of the plan.
        Assert.All(months, m => Assert.Equal(3500m, m.BaselineOutgoingsTotal));

        // Regression diagnostics should be populated
                Assert.False(result.Summary.Expenses.UsingFlatAverage);
        Assert.True(result.Summary.Expenses.RSquared >= 0.99m);
    }

    /// <summary>
    /// Given fewer historical data points than the minimum required
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the regression should fall back to the standard historical baseline
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedTooFewDataPoints_FallsBackToHistoricalBaseline()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 12,
            incomeCorrelatedSettings: new IncomeCorrelatedSettings { MinDataPoints = 6 });

        var mockAccount = HistoricalAccount(accountId, TrainingDataThrough);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 42000 total debits / 12 months = 3500/month
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 42000m }]
            });

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Only 3 months of data — below the MinDataPoints threshold of 6
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (42000 / 12 = 3500)
        Assert.All(result.Months, m => Assert.Equal(3500m, m.BaselineOutgoingsTotal));

                Assert.True(result.Summary.Expenses.UsingFlatAverage);
    }

    /// <summary>
    /// Given historical data with a weak correlation between income and expenses
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the regression should fall back to the standard historical baseline because R² is too low
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedLowRSquared_FallsBackToHistoricalBaseline()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        var mockAccount = HistoricalAccount(accountId, TrainingDataThrough);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 60000 total debits / 12 months = 5000/month
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 60000m }]
            });

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Scatter data with essentially no correlation — expenses are random relative to income
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 8000m), (6000m, 2000m), (7000m, 9000m), (8000m, 1000m), (9000m, 7000m), (10000m, 3000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (60000 / 12 = 5000)
        Assert.All(result.Months, m => Assert.Equal(5000m, m.BaselineOutgoingsTotal));

                Assert.True(result.Summary.Expenses.UsingFlatAverage);
        Assert.True(result.Summary.Expenses.RSquared < 0.5m);
    }

    /// <summary>
    /// Given historical data where expenses decrease as income increases (negative slope)
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the regression should fall back to the standard historical baseline because negative slope is nonsensical
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedNegativeSlope_FallsBackToHistoricalBaseline()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        var mockAccount = HistoricalAccount(accountId, TrainingDataThrough);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 57000 total debits / 12 months = 4750/month
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 57000m }]
            });

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Inverse relationship: as income goes up, expenses go down (negative slope)
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 6000m), (6000m, 5500m), (7000m, 5000m), (8000m, 4500m), (9000m, 4000m), (10000m, 3500m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (57000 / 12 = 4750)
        Assert.All(result.Months, m => Assert.Equal(4750m, m.BaselineOutgoingsTotal));

                Assert.True(result.Summary.Expenses.UsingFlatAverage);
    }

    /// <summary>
    /// Given extra income that ends part-way through the plan
    /// When the forecast is calculated
    /// Then modelled expenses should fall back with it
    /// </summary>
    /// <remarks>
    /// The behaviour the whole expense model exists for. A plan carrying a single flat income figure
    /// could not express this at all: income was the same constant in every month, so the expense
    /// line was flat by construction whatever the fitted slope said. Income is a series now, so a
    /// second job, an allowance or a contract that ends is an income item with an end date, and
    /// spending follows it down.
    /// </remarks>
    [Fact]
    public async Task Calculate_ExtraIncomeEnds_ExpensesFallBackWithIt()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 20000m,
            monthlyIncome: 5000m,          // the salary that carries on
            lookbackMonths: 6);

        // Extra income of 3000 a month that stops after July.
        plan.PlannedItems.Add(MonthlyIncome(planId, 3000m, new DateOnly(2024, 7, 1), until: new DateOnly(2024, 7, 31), name: "Contract work"));

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(accountId, TrainingDataThrough) });

        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Perfect linear relationship: expense = 1000 + 0.5 * income
        // Historical avg income = 7500
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3500m), (6000m, 4000m), (7000m, 4500m), (8000m, 5000m), (9000m, 5500m), (10000m, 6000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // The plan models no income inside the training window (it starts after it), so there is no
        // shortfall to correct for and the fit is read at the modelled income directly.
        // Regression: expense = 1000 + 0.5 x income
        //   July   (8000 = 5000 + 3000): 1000 + 4000 = 5000
        //   August (5000, contract over): 1000 + 2500 = 3500
        Assert.Equal(8000m, months[0].IncomeTotal);
        Assert.Equal(5000m, months[1].IncomeTotal);

        Assert.Equal(5000m, months[0].BaselineOutgoingsTotal);
        Assert.Equal(3500m, months[1].BaselineOutgoingsTotal);
        Assert.Equal(3500m, months[2].BaselineOutgoingsTotal);
    }

    /// <summary>
    /// Given valid regression data from multiple accounts
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the regression should aggregate across all accounts
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedMultipleAccounts_AggregatesAcrossAccounts()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId1, accountId2]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 7, 31),
            startingBalance: 20000m,
            monthlyIncome: 6000m,
            lookbackMonths: 6);

        var account1 = HistoricalAccount(accountId1, TrainingDataThrough, balance: 15000m, name: "Transaction Account");
        var account2 = HistoricalAccount(accountId2, TrainingDataThrough, balance: 5000m, name: "Credit Card");

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { account1, account2 });

        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Account 1: income 4000/month, expenses 2000/month (half of income through each account)
        // Account 2: income 1000/month, expenses 1000/month
        // Combined per month: income = 5000..10000, expense = 3000..5500
        // This is equivalent to the single-account test data
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId1] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(4000m, 2000m), (5000m, 2500m), (6000m, 3000m), (7000m, 3500m), (8000m, 4000m), (9000m, 4500m)]),
                [accountId2] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(1000m, 1000m), (1000m, 1000m), (1000m, 1000m), (1000m, 1000m), (1000m, 1000m), (1000m, 1000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
                Assert.False(result.Summary.Expenses.UsingFlatAverage);
        Assert.True(result.Summary.Expenses.RSquared > 0.5m);
    }

    /// <summary>
    /// Given the HistoricalAverage outgoing mode (default)
    /// When the forecast is calculated
    /// Then no regression diagnostics should be present in the summary
    /// </summary>
    [Fact]
    public async Task Calculate_HistoricalAverageMode_NoRegressionDiagnostics()
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
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Summary.Expenses.UsingFlatAverage);
    }

    /// <summary>
    /// Given historical data with zero variance in income (all months identical)
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the regression should fall back to the standard historical baseline because regression cannot be fitted
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedZeroVariance_FallsBackToHistoricalBaseline()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 7, 1),
            endDate: new DateOnly(2024, 9, 30),
            startingBalance: 10000m,
            monthlyIncome: 5000m,
            lookbackMonths: 12);

        var mockAccount = HistoricalAccount(accountId, TrainingDataThrough);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 36000 total debits / 12 months = 3000/month
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 36000m }]
            });

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // All months have identical income — zero variance, regression denominator = 0
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (5000m, 3200m), (5000m, 2800m), (5000m, 3100m), (5000m, 2900m), (5000m, 3000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (36000 / 12 = 3000)
        Assert.All(result.Months, m => Assert.Equal(3000m, m.BaselineOutgoingsTotal));

                Assert.True(result.Summary.Expenses.UsingFlatAverage);
    }

    /// <summary>
    /// Given a plan whose modelled income stops entirely part-way through
    /// When the forecast is calculated
    /// Then the predicted outgoings should be floored at nought rather than going negative
    /// </summary>
    /// <remarks>
    /// Income can no longer be negative — planned items are validated above zero — so the way to
    /// drive the prediction below the axis is a plan that models a large income for a while and
    /// then none at all, which makes the shortfall correction outweigh the month's own income.
    ///
    /// Worth naming what the floor is hiding: a household with no income does not stop spending, so
    /// nought is not a believable answer either. The honest floor is the fixed component. That is a
    /// change to the expense model rather than to income, so it is deliberately not made here.
    /// </remarks>
    [Fact]
    public async Task Calculate_ModelledIncomeStops_PredictedOutgoingsAreFlooredAtNought()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planId = Guid.NewGuid();
        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 12, 31),
            startingBalance: 10000m,
            monthlyIncome: 0m,
            lookbackMonths: 6);

        // 20,000 a month for the first half of the year, then nothing.
        plan.PlannedItems.Add(MonthlyIncome(planId, 20000m, new DateOnly(2024, 1, 1), until: new DateOnly(2024, 6, 30)));

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(accountId, TrainingDataThrough) });

        SetupEmptyReportMocks();

        // Regression: expense = 1000 + 0.5 x income, average historical income 7500.
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3500m), (6000m, 4000m), (7000m, 4500m), (8000m, 5000m), (9000m, 5500m), (10000m, 6000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // The plan models 20,000 a month across the whole training window, against 7,500 of actual
        // credits, so the shortfall is -12,500. July onwards has no modelled income at all:
        //   1000 + 0.5 x (0 - 12500) = -5250, floored to 0.
        Assert.Equal(0m, months[6].IncomeTotal);
        Assert.Equal(0m, months[6].BaselineOutgoingsTotal);

        // While the income was being modelled the shortfall cancels it back to the historical
        // average, so the earlier months predict off the middle of the fitted line.
        Assert.Equal(20000m, months[0].IncomeTotal);
        Assert.Equal(4750m, months[0].BaselineOutgoingsTotal);
    }

    /// <summary>
    /// Given account data that stops part-way through a month
    /// When the forecast is calculated with a regression
    /// Then that part-month must not be fitted
    /// </summary>
    /// <remarks>
    /// The defect this pins down: the training window used to open at
    /// <c>latestTransactionDate - LookbackMonths</c>, so it began and ended mid-month. In the real
    /// data that put a single day's tail — income $0, expenses $9 — into the training set as though
    /// it were a whole month. A point at the origin anchors the line: it moved the fixed component
    /// from $6,965 to $2,399, the slope from 0.327 to 0.529, and R² from 0.284 to 0.691, and the
    /// inflated fit was the one the forecast ran on.
    ///
    /// Here the six real months are perfectly collinear (expense = 500 + 0.5 x income). A stub month
    /// off that line can only survive by changing the fit, so an unchanged fit proves it was dropped.
    /// </remarks>
    [Fact]
    public async Task Calculate_PartialMonthAtTheEdgeOfTheData_IsNotFitted()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 8, 1),
            endDate: new DateOnly(2024, 8, 31),
            startingBalance: 20000m,
            monthlyIncome: 6000m,
            lookbackMonths: 6);

        // The account's data stops on the 1st of July — one day into the month.
        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(accountId, new DateOnly(2024, 7, 1)) });

        SetupEmptyReportMocks();

        // Six clean months on the line, then July's nine dollars.
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m), (8000m, 4500m), (9000m, 5000m), (10000m, 5500m), (0m, 9m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
                Assert.False(result.Summary.Expenses.UsingFlatAverage);

        // The six real months fit exactly. Including the stub would drag R² off 1.
        Assert.True(result.Summary.Expenses.RSquared >= 0.99m,
            $"the part-month was fitted: R² came out at {result.Summary.Expenses.RSquared}");
        Assert.Equal(0.5m, Math.Round(result.Summary.Expenses.VariableComponent, 4));
        Assert.Equal(500m, Math.Round(result.Summary.Expenses.FixedComponent, 2));
    }

    /// <summary>
    /// Given a savings account with fresher data than the transaction accounts being fitted
    /// When the forecast is calculated with a regression
    /// Then the training window must still close on the transaction accounts' last complete month
    /// </summary>
    /// <remarks>
    /// Savings accounts are excluded from historical analysis, so their transactions never reach the
    /// regression — but the data boundary used to be the maximum across every account, which let a
    /// savings account hold the window open over months the fitted accounts had no data for. That
    /// produces the same empty-month-at-the-origin as a part-month does.
    /// </remarks>
    [Fact]
    public async Task Calculate_SavingsAccountWithFresherData_DoesNotExtendTheTrainingWindow()
    {
        // Arrange
        var transactionAccountId = Guid.NewGuid();
        var savingsAccountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [transactionAccountId, savingsAccountId]));

        var plan = CreatePlanWithStrategies(
            startDate: new DateOnly(2024, 8, 1),
            endDate: new DateOnly(2024, 8, 31),
            startingBalance: 20000m,
            monthlyIncome: 6000m,
            lookbackMonths: 6);

        var savings = new LogicalAccount(savingsAccountId, [])
        {
            Name = "Savings",
            AccountType = AccountType.Savings,
            LastTransaction = new DateOnly(2024, 9, 30), // three months ahead of the fitted account
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(transactionAccountId, TrainingDataThrough), savings });

        SetupEmptyReportMocks();

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [transactionAccountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m), (8000m, 4500m), (9000m, 5000m), (10000m, 5500m)]),
            });

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — July, August and September hold no data for the fitted account and must not appear.
                Assert.False(result.Summary.Expenses.UsingFlatAverage);
        Assert.True(result.Summary.Expenses.RSquared >= 0.99m,
            $"empty months were fitted: R² came out at {result.Summary.Expenses.RSquared}");
    }

    /// <summary>
    /// Given a planned expense that has been paid, and is therefore in the training data
    /// When the forecast is calculated
    /// Then it should not teach the expense model that the household spends that much every month
    /// </summary>
    /// <remarks>
    /// This is the complaint issue #928 opens with: a planned expense appearing in the transaction
    /// log throws out the expense calculations even though it was expected. A $30,000 solar
    /// installation sitting in one training month drags the fitted line up for every month of the
    /// plan, so the forecast charges for it once on its own date and again, smeared, forever.
    /// </remarks>
    [Fact]
    public async Task Calculate_PlannedExpenseThatWasPaid_DoesNotTeachTheModelItRecurs()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        const int solarTag = 42;
        var planId = Guid.NewGuid();

        var plan = CreatePlanWithStrategies(
            id: planId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 8, 31),
            startingBalance: 50000m,
            monthlyIncome: 6000m,
            lookbackMonths: 6);

        plan.PlannedItems.Add(new DomainPlannedItem(Guid.NewGuid())
        {
            ForecastPlanId = planId,
            Name = "Solar",
            ItemType = PlannedItemType.Expense,
            Amount = 30_000m,
            TagId = solarTag,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = new DateOnly(2024, 4, 15) },
        });

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { HistoricalAccount(accountId, TrainingDataThrough) });

        SetupEmptyReportMocks();

        // Six months on the line expense = 500 + 0.5 x income, except April, which also carries the
        // 30,000 solar payment.
        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m), (8000m, 34_500m), (9000m, 5000m), (10000m, 5500m)])
            });

        // The solar payment landed in April, tagged.
        _mocks.SetTaggedSpend(new TaggedSpend(accountId, new DateOnly(2024, 4, 1), solarTag, TransactionType.Debit, 30_000m, InReporting: true));

        var engine = new ForecastEngine(
            _mocks.ReportReaderMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.PlannedItemMatcherMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — with the solar payment taken back out, the six months are exactly on the line
        // again, so the fit is the clean one and not the one the spike would have produced.
                Assert.False(result.Summary.Expenses.UsingFlatAverage);
        Assert.True(result.Summary.Expenses.RSquared >= 0.99m,
            $"the planned expense was left in the training data: R² came out at {result.Summary.Expenses.RSquared}");
        Assert.Equal(0.5m, Math.Round(result.Summary.Expenses.VariableComponent, 4));

        // And the item reports what actually happened rather than what was planned.
        var progress = Assert.Single(result.PlannedItems, p => p.Name == "Solar");
        Assert.Equal(30_000m, progress.ActualToDate);
        Assert.Equal(0m, progress.Remaining);
    }

    #endregion

    /// <summary>
    /// Creates monthly credit/debit total data for testing regression.
    /// Each tuple is (income, expense). Debit totals are stored as negative values
    /// to match the SP sign convention.
    /// </summary>
    private static List<MonthlyCreditDebitTotal> CreateMonthlyData(DateOnly startMonth, IEnumerable<(decimal Income, decimal Expense)> data)
    {
        var result = new List<MonthlyCreditDebitTotal>();
        var currentMonth = startMonth;

        foreach (var (income, expense) in data)
        {
            result.Add(new MonthlyCreditDebitTotal
            {
                Month = currentMonth,
                TransactionType = TransactionFilterType.Credit,
                Total = income,
            });
            result.Add(new MonthlyCreditDebitTotal
            {
                Month = currentMonth,
                TransactionType = TransactionFilterType.Debit,
                Total = -expense, // Negative to match SP sign convention
            });
            currentMonth = currentMonth.AddMonths(1);
        }

        return result;
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
        _mocks.ReportReaderMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        _mocks.ReportReaderMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>());
    }
}
