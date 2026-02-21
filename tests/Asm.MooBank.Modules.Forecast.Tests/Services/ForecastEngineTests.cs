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
        AccountScopeMode accountScopeMode = AccountScopeMode.AllAccounts,
        string outgoingMode = "HistoricalAverage",
        IncomeCorrelatedSettings? incomeCorrelatedSettings = null)
    {
        var incomeStrategy = new IncomeStrategy
        {
            Mode = "ManualRecurring",
            ManualRecurring = new ManualRecurringIncome { Amount = monthlyIncome, Frequency = "Monthly" }
        };

        var outgoingStrategy = new OutgoingStrategy
        {
            Mode = outgoingMode,
            LookbackMonths = lookbackMonths,
            IncomeCorrelated = incomeCorrelatedSettings,
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
            lookbackMonths: 6,
            outgoingMode: "IncomeCorrelated");

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Historical monthly data: strong positive correlation between income and expenses
        // Income: 5000, 6000, 7000, 8000, 9000, 10000
        // Expense: 3000, 3500, 4000, 4500, 5000, 5500
        // Regression: expense = 500 + 0.5 * income, R² = 1.0
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m), (8000m, 4500m), (9000m, 5000m), (10000m, 5500m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(3, months.Count);

        // All months have same income (6000) but regression adjusts outgoings based on
        // income + offset. Avg historical income = 7500, plan income = 6000, offset = 1500
        // Predicted outgoings = 500 + 0.5 * (6000 + 1500) = 500 + 3750 = 4250
        Assert.All(months, m => Assert.Equal(4250m, m.BaselineOutgoingsTotal));

        // Regression diagnostics should be populated
        Assert.NotNull(result.Summary.Regression);
        Assert.False(result.Summary.Regression.FellBackToFlatAverage);
        Assert.True(result.Summary.Regression.RSquared >= 0.99m);
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
            outgoingMode: "IncomeCorrelated",
            incomeCorrelatedSettings: new IncomeCorrelatedSettings { MinDataPoints = 6 });

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 42000 total debits / 12 months = 3500/month
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 42000m }]
            });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Only 3 months of data — below the MinDataPoints threshold of 6
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (6000m, 3500m), (7000m, 4000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (42000 / 12 = 3500)
        Assert.All(result.Months, m => Assert.Equal(3500m, m.BaselineOutgoingsTotal));

        Assert.NotNull(result.Summary.Regression);
        Assert.True(result.Summary.Regression.FellBackToFlatAverage);
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
            lookbackMonths: 12,
            outgoingMode: "IncomeCorrelated");

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 60000 total debits / 12 months = 5000/month
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 60000m }]
            });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Scatter data with essentially no correlation — expenses are random relative to income
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 8000m), (6000m, 2000m), (7000m, 9000m), (8000m, 1000m), (9000m, 7000m), (10000m, 3000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (60000 / 12 = 5000)
        Assert.All(result.Months, m => Assert.Equal(5000m, m.BaselineOutgoingsTotal));

        Assert.NotNull(result.Summary.Regression);
        Assert.True(result.Summary.Regression.FellBackToFlatAverage);
        Assert.True(result.Summary.Regression.RSquared < 0.5m);
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
            lookbackMonths: 12,
            outgoingMode: "IncomeCorrelated");

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 57000 total debits / 12 months = 4750/month
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 57000m }]
            });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Inverse relationship: as income goes up, expenses go down (negative slope)
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 6000m), (6000m, 5500m), (7000m, 5000m), (8000m, 4500m), (9000m, 4000m), (10000m, 3500m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (57000 / 12 = 4750)
        Assert.All(result.Months, m => Assert.Equal(4750m, m.BaselineOutgoingsTotal));

        Assert.NotNull(result.Summary.Regression);
        Assert.True(result.Summary.Regression.FellBackToFlatAverage);
    }

    /// <summary>
    /// Given a valid regression and income that drops between months
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the outgoings should decrease proportionally with lower income
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedWithIncomeAdjustment_OutgoingsFollowIncome()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        // Plan with income starting at 8000, then dropping by 3000 in month 2
        var incomeStrategy = new IncomeStrategy
        {
            Mode = "ManualRecurring",
            ManualRecurring = new ManualRecurringIncome { Amount = 8000m, Frequency = "Monthly" },
            ManualAdjustments =
            [
                new ManualAdjustment { Date = new DateOnly(2024, 8, 1), DeltaAmount = -3000m }
            ],
        };

        var outgoingStrategy = new OutgoingStrategy
        {
            Mode = "IncomeCorrelated",
            LookbackMonths = 6,
        };

        var plan = new DomainForecastPlan(Guid.NewGuid())
        {
            FamilyId = _mocks.User.FamilyId,
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 9, 30),
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 20000m,
            AccountScopeMode = AccountScopeMode.AllAccounts,
            CurrencyCode = "AUD",
            IncomeStrategySerialized = JsonSerializer.Serialize(incomeStrategy, JsonOptions),
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Perfect linear relationship: expense = 1000 + 0.5 * income
        // Historical avg income = 7500
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3500m), (6000m, 4000m), (7000m, 4500m), (8000m, 5000m), (9000m, 5500m), (10000m, 6000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var months = result.Months.ToList();

        // Regression: expense = 1000 + 0.5 * income
        // Historical avg income = 7500, plan base income = 8000, offset = -500
        // Month 1 (income 8000): 1000 + 0.5 * (8000 + (-500)) = 1000 + 3750 = 4750
        // Month 2 (income 5000): 1000 + 0.5 * (5000 + (-500)) = 1000 + 2250 = 3250
        // Month 3 (income 5000): same as month 2 = 3250
        Assert.Equal(4750m, months[0].BaselineOutgoingsTotal);
        Assert.Equal(3250m, months[1].BaselineOutgoingsTotal);
        Assert.Equal(3250m, months[2].BaselineOutgoingsTotal);

        // Outgoings should be lower in months with lower income
        Assert.True(months[1].BaselineOutgoingsTotal < months[0].BaselineOutgoingsTotal);
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
            lookbackMonths: 6,
            outgoingMode: "IncomeCorrelated");

        var account1 = new LogicalAccount(accountId1, [])
        {
            Name = "Transaction Account",
            Balance = 15000m,
            AccountType = AccountType.Transaction,
        };
        var account2 = new LogicalAccount(accountId2, [])
        {
            Name = "Credit Card",
            Balance = 5000m,
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { account1, account2 });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Account 1: income 4000/month, expenses 2000/month (half of income through each account)
        // Account 2: income 1000/month, expenses 1000/month
        // Combined per month: income = 5000..10000, expense = 3000..5500
        // This is equivalent to the single-account test data
        _mocks.ReportRepositoryMock
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
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result.Summary.Regression);
        Assert.False(result.Summary.Regression.FellBackToFlatAverage);
        Assert.True(result.Summary.Regression.RSquared > 0.5m);
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
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.Summary.Regression);
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
            lookbackMonths: 12,
            outgoingMode: "IncomeCorrelated");

        var mockAccount = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument> { mockAccount });

        // Historical baseline: 36000 total debits / 12 months = 3000/month
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>
            {
                [accountId] = [new() { TransactionType = TransactionFilterType.Debit, Total = 36000m }]
            });

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // All months have identical income — zero variance, regression denominator = 0
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3000m), (5000m, 3200m), (5000m, 2800m), (5000m, 3100m), (5000m, 2900m), (5000m, 3000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert — falls back to historical baseline (36000 / 12 = 3000)
        Assert.All(result.Months, m => Assert.Equal(3000m, m.BaselineOutgoingsTotal));

        Assert.NotNull(result.Summary.Regression);
        Assert.True(result.Summary.Regression.FellBackToFlatAverage);
    }

    /// <summary>
    /// Given a valid regression where an income adjustment drops income far below the base
    /// When the forecast is calculated with IncomeCorrelated mode
    /// Then the outgoings should be floored at zero (not go negative)
    /// </summary>
    [Fact]
    public async Task Calculate_IncomeCorrelatedNegativePrediction_FloorsAtZero()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        // Plan with base income 8000, then a -10000 adjustment in month 1 → effective month income = -2000
        // The offset is computed from planBaseIncome (8000), so the regression input will be:
        //   monthIncome + offset = -2000 + (avgHistorical - 8000)
        // With avgHistorical = 7500, offset = -500, effective = -2500
        // Regression: 1000 + 0.5 * (-2500) = -250 → floored to 0
        var incomeStrategy = new IncomeStrategy
        {
            Mode = "ManualRecurring",
            ManualRecurring = new ManualRecurringIncome { Amount = 8000m, Frequency = "Monthly" },
            ManualAdjustments =
            [
                new ManualAdjustment { Date = new DateOnly(2024, 7, 1), DeltaAmount = -10000m }
            ],
        };

        var outgoingStrategy = new OutgoingStrategy
        {
            Mode = "IncomeCorrelated",
            LookbackMonths = 6,
        };

        var plan = new DomainForecastPlan(Guid.NewGuid())
        {
            FamilyId = _mocks.User.FamilyId,
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 31),
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            AccountScopeMode = AccountScopeMode.AllAccounts,
            CurrencyCode = "AUD",
            IncomeStrategySerialized = JsonSerializer.Serialize(incomeStrategy, JsonOptions),
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainInstrument>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<CreditDebitTotal>>());

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyBalancesForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyBalance>>());

        // Regression: expense = 1000 + 0.5 * income, avg income = 7500
        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>
            {
                [accountId] = CreateMonthlyData(
                    new DateOnly(2024, 1, 1),
                    [(5000m, 3500m), (6000m, 4000m), (7000m, 4500m), (8000m, 5000m), (9000m, 5500m), (10000m, 6000m)])
            });

        var engine = new ForecastEngine(
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.User);

        // Act
        var result = await engine.Calculate(plan, TestContext.Current.CancellationToken);

        // Assert
        var month = result.Months.First();
        Assert.Equal(0m, month.BaselineOutgoingsTotal); // Floored at zero
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

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetMonthlyCreditDebitTotalsForAccounts(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>>());
    }
}
