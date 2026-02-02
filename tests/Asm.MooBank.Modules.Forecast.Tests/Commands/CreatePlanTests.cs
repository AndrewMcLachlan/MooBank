#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class CreatePlanTests
{
    private readonly TestMocks _mocks;

    public CreatePlanTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPlan()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            CurrencyCode = "AUD",
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m, Frequency = "Monthly" }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Plan", result.Name);
        Assert.NotNull(capturedPlan);
        Assert.Equal("Test Plan", capturedPlan.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsFamilyId()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.Equal(familyId, capturedPlan.FamilyId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsDates()
    {
        // Arrange
        var startDate = new DateOnly(2024, 3, 1);
        var endDate = new DateOnly(2025, 2, 28);
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = startDate,
            EndDate = endDate,
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.NotNull(capturedPlan);
        Assert.Equal(startDate, capturedPlan.StartDate);
        Assert.Equal(endDate, capturedPlan.EndDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.ForecastRepositoryMock.Verify(r => r.Add(It.IsAny<DomainForecastPlan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoIncomeStrategy_CalculatesHistoricalIncome()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 6, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            // No income strategy - should calculate from history
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        _mocks.ReportRepositoryMock
            .Setup(r => r.GetCreditDebitTotals(accountId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditDebitTotal>
            {
                new() { TransactionType = TransactionFilterType.Credit, Total = 60000m },
            });

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.NotNull(capturedPlan.IncomeStrategySerialized);
        Assert.Contains("5000", capturedPlan.IncomeStrategySerialized); // 60000 / 12 = 5000
    }

    [Fact]
    public async Task Handle_NoOutgoingStrategy_SetsDefaultLookback()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            // No outgoing strategy - should default to 12 month lookback
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.NotNull(capturedPlan.OutgoingStrategySerialized);
        Assert.Contains("12", capturedPlan.OutgoingStrategySerialized);
    }

    [Fact]
    public async Task Handle_CalculatedStartingBalance_CalculatesFromAccounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.CalculatedCurrent,
            // No starting balance amount - should calculate from accounts
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        var mockInstrument = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            Balance = 15000m,
            AccountType = AccountType.Transaction,
        };

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockInstrument);

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.Equal(15000m, capturedPlan.StartingBalanceAmount);
    }

    [Fact]
    public async Task Handle_SelectedAccounts_SetsAccountIds()
    {
        // Arrange
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.SelectedAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            AccountIds = [accountId1, accountId2],
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.Equal(2, capturedPlan.Accounts.Count);
    }

    [Fact]
    public async Task Handle_NoCurrency_UsesUserCurrency()
    {
        // Arrange
        _mocks.SetUser(TestMocks.CreateTestUser(currency: "USD"));

        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            // No currency code - should use user's currency
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.Equal("USD", capturedPlan.CurrencyCode);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsTimestamps()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "Test Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        DomainForecastPlan? capturedPlan = null;
        _mocks.ForecastRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainForecastPlan>()))
            .Callback<DomainForecastPlan>(p => capturedPlan = p);

        var beforeCreate = DateTime.UtcNow;

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedPlan);
        Assert.True(capturedPlan.CreatedUtc >= beforeCreate && capturedPlan.CreatedUtc <= afterCreate);
        Assert.True(capturedPlan.UpdatedUtc >= beforeCreate && capturedPlan.UpdatedUtc <= afterCreate);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedPlan()
    {
        // Arrange
        var planModel = new ForecastPlan
        {
            Name = "My New Plan",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.ManualAmount,
            StartingBalanceAmount = 10000m,
            CurrencyCode = "AUD",
            IncomeStrategy = new IncomeStrategy
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = 5000m }
            },
            OutgoingStrategy = new OutgoingStrategy { LookbackMonths = 12 },
        };

        var handler = new CreatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.ReportRepositoryMock.Object,
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new CreatePlan(planModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("My New Plan", result.Name);
        Assert.Equal(new DateOnly(2024, 1, 1), result.StartDate);
        Assert.Equal(new DateOnly(2024, 12, 31), result.EndDate);
        Assert.Equal(AccountScopeMode.AllAccounts, result.AccountScopeMode);
        Assert.Equal(StartingBalanceMode.ManualAmount, result.StartingBalanceMode);
        Assert.Equal(10000m, result.StartingBalanceAmount);
        Assert.Equal("AUD", result.CurrencyCode);
        Assert.False(result.IsArchived);
    }
}
