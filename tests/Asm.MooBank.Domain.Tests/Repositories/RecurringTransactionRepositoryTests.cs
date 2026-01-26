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

    private RecurringTransaction CreateRecurringTransaction(Guid id, string description, decimal amount) =>
        new(id)
        {
            Description = description,
            Amount = amount,
            VirtualAccountId = _virtualAccountId,
            Schedule = ScheduleFrequency.Monthly,
            NextRun = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
        };
}
