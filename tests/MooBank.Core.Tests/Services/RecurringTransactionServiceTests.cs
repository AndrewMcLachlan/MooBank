#nullable enable

using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Microsoft.Extensions.Logging;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using ITransactionRepository = Asm.MooBank.Domain.Entities.Transactions.ITransactionRepository;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="RecurringTransactionService"/> service.
/// Tests cover processing of recurring transactions with various schedules.
/// </summary>
public class RecurringTransactionServiceTests
{
    #region Process

    /// <summary>
    /// Given no recurring transactions
    /// When Process is called
    /// Then no transactions should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_NoRecurringTransactions_CreatesNothing()
    {
        // Arrange
        var (service, transactionRepoMock, _) = CreateService([]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        transactionRepoMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
    }

    /// <summary>
    /// Given a daily recurring transaction due today
    /// When Process is called
    /// Then a transaction should be created and NextRun updated to tomorrow
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_DailyTransactionDueToday_CreatesTransactionAndUpdatesNextRun()
    {
        // Arrange
        var today = DateTime.UtcNow.ToDateOnly();
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Daily, today, 100m, "Daily Transfer");

        var (service, transactionRepoMock, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        transactionRepoMock.Verify(r => r.Add(It.Is<DomainTransaction>(t => t.Amount == 100m)), Times.Once);
        Assert.Equal(today.AddDays(1), recurring.NextRun);
        Assert.NotNull(recurring.LastRun);
    }

    /// <summary>
    /// Given a weekly recurring transaction due today
    /// When Process is called
    /// Then NextRun should be updated to 7 days from now
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_WeeklyTransaction_UpdatesNextRunBy7Days()
    {
        // Arrange
        var today = DateTime.UtcNow.ToDateOnly();
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Weekly, today, 200m, "Weekly");

        var (service, _, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(today.AddDays(7), recurring.NextRun);
    }

    /// <summary>
    /// Given a monthly recurring transaction due today
    /// When Process is called
    /// Then NextRun should be updated to next month
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_MonthlyTransaction_UpdatesNextRunByOneMonth()
    {
        // Arrange
        var today = DateTime.UtcNow.ToDateOnly();
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Monthly, today, 500m, "Monthly");

        var (service, _, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(today.AddMonths(1), recurring.NextRun);
    }

    /// <summary>
    /// Given a recurring transaction not yet due
    /// When Process is called
    /// Then no transaction should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_TransactionNotYetDue_CreatesNothing()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.ToDateOnly().AddDays(1);
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Daily, tomorrow, 100m, "Future");

        var (service, transactionRepoMock, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        transactionRepoMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
    }

    /// <summary>
    /// Given a recurring transaction that missed multiple days
    /// When Process is called
    /// Then multiple transactions should be created to catch up
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_MissedMultipleDays_CatchesUp()
    {
        // Arrange - Transaction was due 3 days ago
        var threeDaysAgo = DateTime.UtcNow.ToDateOnly().AddDays(-3);
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Daily, threeDaysAgo, 50m, "Missed");

        var (service, transactionRepoMock, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert - Should create 4 transactions (3 days ago, 2 days ago, yesterday, today)
        transactionRepoMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Exactly(4));
    }

    /// <summary>
    /// Given multiple recurring transactions
    /// When Process is called
    /// Then all due transactions should be processed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_MultipleTransactions_ProcessesAllDue()
    {
        // Arrange
        var today = DateTime.UtcNow.ToDateOnly();
        var tomorrow = today.AddDays(1);
        var transactions = new List<RecurringTransaction>
        {
            CreateRecurringTransaction(ScheduleFrequency.Daily, today, 100m, "Due Today"),
            CreateRecurringTransaction(ScheduleFrequency.Daily, today, 200m, "Also Due Today"),
            CreateRecurringTransaction(ScheduleFrequency.Daily, tomorrow, 300m, "Due Tomorrow"),
        };

        var (service, transactionRepoMock, _) = CreateService(transactions);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert - Only 2 transactions should be created (the ones due today)
        transactionRepoMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Exactly(2));
    }

    /// <summary>
    /// Given recurring transactions are processed
    /// When Process completes
    /// Then SaveChangesAsync should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_Always_SavesChanges()
    {
        // Arrange
        var (service, _, unitOfWorkMock) = CreateService([]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Given a transaction with null description
    /// When Process is called
    /// Then the transaction should still be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Process_NullDescription_CreatesTransaction()
    {
        // Arrange
        var today = DateTime.UtcNow.ToDateOnly();
        var recurring = CreateRecurringTransaction(ScheduleFrequency.Daily, today, 100m, null);

        var (service, transactionRepoMock, _) = CreateService([recurring]);

        // Act
        await service.Process(TestContext.Current.CancellationToken);

        // Assert
        transactionRepoMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    #endregion

    private static RecurringTransaction CreateRecurringTransaction(
        ScheduleFrequency schedule,
        DateOnly nextRun,
        decimal amount,
        string? description)
    {
        return new RecurringTransaction(Guid.NewGuid())
        {
            VirtualAccountId = Guid.NewGuid(),
            Schedule = schedule,
            NextRun = nextRun,
            Amount = amount,
            Description = description,
        };
    }

    private static (IRecurringTransactionService service, Mock<ITransactionRepository> transactionRepoMock, Mock<IUnitOfWork> unitOfWorkMock) CreateService(
        List<RecurringTransaction> recurringTransactions)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var transactionRepoMock = new Mock<ITransactionRepository>();
        var recurringRepoMock = new Mock<IRecurringTransactionRepository>();
        var loggerMock = new Mock<ILogger<RecurringTransactionService>>();

        recurringRepoMock.Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync(recurringTransactions);

        var service = new RecurringTransactionService(
            unitOfWorkMock.Object,
            transactionRepoMock.Object,
            recurringRepoMock.Object,
            loggerMock.Object);

        return (service, transactionRepoMock, unitOfWorkMock);
    }
}
