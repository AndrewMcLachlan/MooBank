#nullable enable
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

/// <summary>
/// Unit tests for deleting a <see cref="Transaction"/>.
/// </summary>
/// <remarks>
/// The interesting case is a refund. Splits and their tags go with the transaction because the
/// database cascades them, but the offsets a refund records against *other* transactions' splits
/// have no cascade of their own — a second cascade path from Transaction would be rejected — so the
/// aggregate clears them itself. Left behind, they hold a net amount down against a transaction
/// that no longer exists.
/// </remarks>
[Trait("Category", "Unit")]
public class TransactionRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly Guid _accountId = Guid.NewGuid();

    public TransactionRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private TransactionRepository CreateRepository() => new(_context);

    private Transaction AddTransaction(decimal amount, params TransactionSplit[] splits)
    {
        var transaction = new Transaction(Guid.NewGuid())
        {
            AccountId = _accountId,
            Amount = amount,
            Description = "Test",
            TransactionTime = DateTime.UtcNow,
            TransactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit,
            Source = "Test",
        };

        _context.Add(transaction);

        foreach (var split in splits)
        {
            split.TransactionId = transaction.Id;
            _context.Add(split);
        }

        return transaction;
    }

    /// <summary>
    /// Given a refund that offsets a purchase
    /// When the refund is deleted
    /// Then the offset recorded against the purchase should go with it
    /// </summary>
    [Fact]
    public async Task Delete_RefundTransaction_RemovesItsOffsetsFromOtherSplits()
    {
        // Arrange
        var purchaseSplit = new TransactionSplit(Guid.NewGuid()) { Amount = 100m };
        AddTransaction(-100m, purchaseSplit);

        var refund = AddTransaction(30m);

        _context.Add(new TransactionOffset
        {
            TransactionSplitId = purchaseSplit.Id,
            OffsetTransactionId = refund.Id,
            Amount = 30m,
        });

        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        var repository = CreateRepository();
        var loaded = await repository.Get(refund.Id, new IncludeSplitsAndOffsetsSpecification(), TestContext.Current.CancellationToken);

        Assert.Single(loaded.OffsetFor);

        // Act
        loaded.Delete();
        repository.Delete(loaded);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(await _context.Set<TransactionOffset>().ToListAsync(TestContext.Current.CancellationToken));
        Assert.Null(await _context.Set<Transaction>().FindAsync([refund.Id], TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given a refund that offsets a purchase
    /// When the refund is deleted
    /// Then the purchase and its split should survive
    /// </summary>
    [Fact]
    public async Task Delete_RefundTransaction_LeavesTheOffsetTransactionIntact()
    {
        // Arrange
        var purchaseSplit = new TransactionSplit(Guid.NewGuid()) { Amount = 100m };
        var purchase = AddTransaction(-100m, purchaseSplit);

        var refund = AddTransaction(30m);

        _context.Add(new TransactionOffset
        {
            TransactionSplitId = purchaseSplit.Id,
            OffsetTransactionId = refund.Id,
            Amount = 30m,
        });

        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        var repository = CreateRepository();
        var loaded = await repository.Get(refund.Id, new IncludeSplitsAndOffsetsSpecification(), TestContext.Current.CancellationToken);

        // Act
        loaded.Delete();
        repository.Delete(loaded);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(await _context.Set<Transaction>().FindAsync([purchase.Id], TestContext.Current.CancellationToken));
        Assert.NotNull(await _context.Set<TransactionSplit>().FindAsync([purchaseSplit.Id], TestContext.Current.CancellationToken));
    }
}
