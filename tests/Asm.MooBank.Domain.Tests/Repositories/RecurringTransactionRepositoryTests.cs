using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="RecurringTransactionRepository"/> class.
/// Tests verify recurring transaction read operations against an in-memory database.
/// </summary>
public class RecurringTransactionRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _virtualAccountId = Guid.NewGuid();
    private readonly Guid _parentAccountId = Guid.NewGuid();

    public RecurringTransactionRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        SetupVirtualAccount();
    }

    private void SetupVirtualAccount()
    {
        var parentAccount = new LogicalAccount(_parentAccountId, [])
        {
            Name = "Parent Account",
            Currency = "AUD",
        };
        _context.Set<LogicalAccount>().Add(parentAccount);

        var virtualAccount = new DomainVirtualInstrument(_virtualAccountId)
        {
            Name = "Test Virtual Account",
            Currency = "AUD",
            ParentInstrumentId = _parentAccountId,
        };
        _context.Set<DomainVirtualInstrument>().Add(virtualAccount);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given recurring transactions exist
    /// When Get is called
    /// Then all recurring transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingTransactions_ReturnsAll()
    {
        // Arrange
        var rt1 = CreateRecurringTransaction(Guid.NewGuid(), "Rent", 1500m);
        var rt2 = CreateRecurringTransaction(Guid.NewGuid(), "Utilities", 200m);

        _context.Set<RecurringTransaction>().AddRange(rt1, rt2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RecurringTransactionRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given a recurring transaction exists
    /// When Get by id is called
    /// Then the recurring transaction should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingTransaction_ReturnsTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = CreateRecurringTransaction(transactionId, "Test Recurring", 500m);
        _context.Set<RecurringTransaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new RecurringTransactionRepository(_context);

        // Act
        var result = await repository.Get(transactionId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Recurring", result.Description);
        Assert.Equal(500m, result.Amount);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new recurring transaction
    /// When added to context
    /// Then the transaction should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewTransaction_PersistsTransaction()
    {
        // Arrange
        var transaction = CreateRecurringTransaction(Guid.NewGuid(), "New Recurring", 750m);

        // Act
        _context.Set<RecurringTransaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var saved = await _context.Set<RecurringTransaction>()
            .FirstOrDefaultAsync(t => t.Description == "New Recurring", TestContext.Current.CancellationToken);
        Assert.NotNull(saved);
        Assert.Equal(750m, saved.Amount);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing recurring transaction
    /// When properties are updated
    /// Then changes should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingTransaction_UpdatesProperties()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = CreateRecurringTransaction(transactionId, "Original", 500m);
        _context.Set<RecurringTransaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        transaction.Description = "Updated";
        transaction.Amount = 600m;
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updated = await _context.Set<RecurringTransaction>()
            .FindAsync([transactionId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated", updated!.Description);
        Assert.Equal(600m, updated.Amount);
    }

    /// <summary>
    /// Given a recurring transaction
    /// When LastRun is updated
    /// Then LastRun should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_LastRun_PersistsLastRunDate()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = CreateRecurringTransaction(transactionId, "Test", 500m);
        _context.Set<RecurringTransaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var lastRunDate = DateTime.UtcNow;

        // Act
        transaction.LastRun = lastRunDate;
        transaction.NextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updated = await _context.Set<RecurringTransaction>()
            .FindAsync([transactionId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updated!.LastRun);
        Assert.Equal(lastRunDate.Date, updated.LastRun.Value.Date);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given an existing recurring transaction
    /// When removed from context
    /// Then the transaction should be deleted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingTransaction_RemovesTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = CreateRecurringTransaction(transactionId, "To Delete", 500m);
        _context.Set<RecurringTransaction>().Add(transaction);
        await _context.SaveChangesAsync(true, TestContext.Current.CancellationToken);

        // Act
        _context.Set<RecurringTransaction>().Remove(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deleted = await _context.Set<RecurringTransaction>()
            .FindAsync([transactionId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deleted);
    }

    #endregion

    #region Schedule Frequencies

    /// <summary>
    /// Given recurring transactions with different schedules
    /// When querying by schedule
    /// Then transactions with matching schedule should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_FilterBySchedule_ReturnsMatchingSchedule()
    {
        // Arrange
        var monthlyTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Monthly", 1000m, ScheduleFrequency.Monthly);
        var weeklyTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Weekly", 100m, ScheduleFrequency.Weekly);
        var fortnightlyTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Fortnightly", 500m, ScheduleFrequency.Fortnightly);

        _context.Set<RecurringTransaction>().AddRange(monthlyTransaction, weeklyTransaction, fortnightlyTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _context.Set<RecurringTransaction>()
            .Where(t => t.Schedule == ScheduleFrequency.Monthly)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Monthly", result.First().Description);
    }

    #endregion

    #region NextRun Queries

    /// <summary>
    /// Given recurring transactions with different NextRun dates
    /// When querying for due transactions
    /// Then only transactions due should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_DueTransactions_ReturnsOnlyDue()
    {
        // Arrange
        var dueTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Due Today", 500m);
        dueTransaction.NextRun = DateOnly.FromDateTime(DateTime.UtcNow);

        var futureTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Future", 500m);
        futureTransaction.NextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var pastDueTransaction = CreateRecurringTransaction(Guid.NewGuid(), "Past Due", 500m);
        pastDueTransaction.NextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

        _context.Set<RecurringTransaction>().AddRange(dueTransaction, futureTransaction, pastDueTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = await _context.Set<RecurringTransaction>()
            .Where(t => t.NextRun <= today)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Description == "Due Today");
        Assert.Contains(result, t => t.Description == "Past Due");
    }

    #endregion

    private RecurringTransaction CreateRecurringTransaction(Guid id, string description, decimal amount, ScheduleFrequency schedule = ScheduleFrequency.Monthly) =>
        new(id)
        {
            Description = description,
            Amount = amount,
            VirtualAccountId = _virtualAccountId,
            Schedule = schedule,
            NextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
        };
}
