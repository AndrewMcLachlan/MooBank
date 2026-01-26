using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="TransactionRepository"/> class.
/// Tests verify transaction operations against an in-memory database.
/// </summary>
public class TransactionRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _instrumentId = Guid.NewGuid();
    private readonly Guid _institutionAccountId = Guid.NewGuid();

    public TransactionRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        SetupInstrument();
    }

    private void SetupInstrument()
    {
        var instrument = new LogicalAccount(_instrumentId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
        };
        _context.Set<LogicalAccount>().Add(instrument);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetTransactions by InstrumentId

    /// <summary>
    /// Given transactions exist for an instrument
    /// When GetTransactions is called
    /// Then all transactions for that instrument should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_WithExistingTransactions_ReturnsTransactionsForInstrument()
    {
        // Arrange
        var transaction1 = CreateTransaction(_instrumentId, -50m, "Transaction 1");
        var transaction2 = CreateTransaction(_instrumentId, 100m, "Transaction 2");
        var otherInstrumentTransaction = CreateTransaction(Guid.NewGuid(), -25m, "Other");

        _context.Set<Transaction>().AddRange(transaction1, transaction2, otherInstrumentTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TransactionRepository(_context);

        // Act
        var result = await repository.GetTransactions(_instrumentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(_instrumentId, t.AccountId));
    }

    /// <summary>
    /// Given no transactions exist for an instrument
    /// When GetTransactions is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_NoTransactions_ReturnsEmpty()
    {
        // Arrange
        var repository = new TransactionRepository(_context);

        // Act
        var result = await repository.GetTransactions(_instrumentId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given transactions with splits exist
    /// When GetTransactions is called
    /// Then transactions should include splits
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_WithSplits_IncludesSplits()
    {
        // Arrange
        var transaction = CreateTransaction(_instrumentId, -100m, "With Splits");
        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TransactionRepository(_context);

        // Act
        var result = (await repository.GetTransactions(_instrumentId, TestContext.Current.CancellationToken)).First();

        // Assert
        Assert.NotEmpty(result.Splits);
    }

    #endregion

    #region GetTransactions by InstrumentId and InstitutionAccountId

    /// <summary>
    /// Given transactions exist for an institution account
    /// When GetTransactions is called with both ids
    /// Then only transactions matching both should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_ByBothIds_ReturnsMatchingTransactions()
    {
        // Arrange
        var transaction1 = CreateTransaction(_instrumentId, -50m, "Match 1", _institutionAccountId);
        var transaction2 = CreateTransaction(_instrumentId, 100m, "Match 2", _institutionAccountId);
        var differentAccountTransaction = CreateTransaction(_instrumentId, -25m, "Different", Guid.NewGuid());

        _context.Set<Transaction>().AddRange(transaction1, transaction2, differentAccountTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TransactionRepository(_context);

        // Act
        var result = await repository.GetTransactions(_instrumentId, _institutionAccountId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(_institutionAccountId, t.InstitutionAccountId));
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new transaction
    /// When Add is called
    /// Then the transaction should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewTransaction_PersistsTransaction()
    {
        // Arrange
        var repository = new TransactionRepository(_context);
        var transaction = CreateTransaction(_instrumentId, -75m, "New Transaction");

        // Act
        repository.Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedTransaction = await _context.Set<Transaction>().FirstOrDefaultAsync(t => t.Description == "New Transaction", TestContext.Current.CancellationToken);
        Assert.NotNull(savedTransaction);
        Assert.Equal(-75m, savedTransaction.Amount);
    }

    #endregion

    #region Get by Id

    /// <summary>
    /// Given a transaction exists
    /// When Get by id is called
    /// Then the transaction should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingTransaction_ReturnsTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction(transactionId)
        {
            AccountId = _instrumentId,
            Amount = -100m,
            Description = "Test Transaction",
            TransactionTime = DateTime.UtcNow,
            TransactionType = TransactionType.Debit,
            Source = "Test",
        };
        transaction.EnsureMinimumSplit();

        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TransactionRepository(_context);

        // Act
        var result = await repository.Get(transactionId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Transaction", result.Description);
    }

    #endregion

    private static Transaction CreateTransaction(Guid instrumentId, decimal amount, string description, Guid? institutionAccountId = null)
    {
        var transaction = new Transaction(Guid.NewGuid())
        {
            AccountId = instrumentId,
            InstitutionAccountId = institutionAccountId,
            Amount = amount,
            Description = description,
            TransactionTime = DateTime.UtcNow,
            TransactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit,
            Source = "Test",
        };

        transaction.EnsureMinimumSplit();

        return transaction;
    }
}
