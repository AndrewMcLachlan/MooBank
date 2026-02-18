using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

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

    #region Filter by Date Range

    /// <summary>
    /// Given transactions with different dates
    /// When filtering by date range
    /// Then only transactions within range should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_WithDateFilter_ReturnsTransactionsInRange()
    {
        // Arrange
        var oldTransaction = CreateTransaction(_instrumentId, -50m, "Old", transactionTime: DateTime.UtcNow.AddDays(-30));
        var recentTransaction = CreateTransaction(_instrumentId, -100m, "Recent", transactionTime: DateTime.UtcNow.AddDays(-5));
        var futureTransaction = CreateTransaction(_instrumentId, -75m, "Future", transactionTime: DateTime.UtcNow.AddDays(10));

        _context.Set<Transaction>().AddRange(oldTransaction, recentTransaction, futureTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TransactionRepository(_context);
        var filter = new TransactionFilter
        {
            InstrumentId = _instrumentId,
            Start = DateTime.UtcNow.AddDays(-10),
            End = DateTime.UtcNow.AddDays(5),
        };

        // Act
        var query = _context.Set<Transaction>().AsQueryable();
        var spec = new FilterSpecification(filter);
        var result = await spec.Apply(query).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Recent", result.First().Description);
    }

    #endregion

    #region Filter by Transaction Type

    /// <summary>
    /// Given debit and credit transactions
    /// When filtering by debit type
    /// Then only debit transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_FilterByDebit_ReturnsOnlyDebits()
    {
        // Arrange
        var debitTransaction = CreateTransaction(_instrumentId, -100m, "Debit");
        var creditTransaction = CreateTransaction(_instrumentId, 200m, "Credit");

        _context.Set<Transaction>().AddRange(debitTransaction, creditTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var filter = new TransactionFilter
        {
            InstrumentId = _instrumentId,
            TransactionType = TransactionFilterType.Debit,
        };

        // Act
        var query = _context.Set<Transaction>().AsQueryable();
        var spec = new FilterSpecification(filter);
        var result = await spec.Apply(query).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Debit", result.First().Description);
    }

    /// <summary>
    /// Given debit and credit transactions
    /// When filtering by credit type
    /// Then only credit transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_FilterByCredit_ReturnsOnlyCredits()
    {
        // Arrange
        var debitTransaction = CreateTransaction(_instrumentId, -100m, "Debit");
        var creditTransaction = CreateTransaction(_instrumentId, 200m, "Credit");

        _context.Set<Transaction>().AddRange(debitTransaction, creditTransaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var filter = new TransactionFilter
        {
            InstrumentId = _instrumentId,
            TransactionType = TransactionFilterType.Credit,
        };

        // Act
        var query = _context.Set<Transaction>().AsQueryable();
        var spec = new FilterSpecification(filter);
        var result = await spec.Apply(query).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Credit", result.First().Description);
    }

    #endregion

    #region Filter by Description

    /// <summary>
    /// Given transactions with different descriptions
    /// When filtering by description
    /// Then only matching transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransactions_FilterByDescription_ReturnsMatching()
    {
        // Arrange
        var groceryTransaction = CreateTransaction(_instrumentId, -50m, "Groceries at Costco");
        var gasTransaction = CreateTransaction(_instrumentId, -30m, "Gas Station");
        var anotherGrocery = CreateTransaction(_instrumentId, -75m, "Grocery Store");

        _context.Set<Transaction>().AddRange(groceryTransaction, gasTransaction, anotherGrocery);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var filter = new TransactionFilter
        {
            InstrumentId = _instrumentId,
            Filter = "Grocer",
        };

        // Act
        var query = _context.Set<Transaction>().AsQueryable();
        var spec = new FilterSpecification(filter);
        var result = await spec.Apply(query).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Contains("Grocer", t.Description, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Transaction with Tags

    /// <summary>
    /// Given a transaction with tagged splits
    /// When querying with includes
    /// Then tags should be loaded
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTransaction_WithTags_LoadsTags()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family(familyId) { Name = "Test Family" };
        _context.Set<Family>().Add(family);

        var tag = new DomainTag(1) { Name = "Food", FamilyId = familyId };
        _context.Set<DomainTag>().Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var transaction = CreateTransaction(_instrumentId, -50m, "Lunch");
        transaction.Splits.First().Tags.Add(tag);
        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _context.Set<Transaction>()
            .Include(t => t.Splits)
                .ThenInclude(s => s.Tags)
            .FirstAsync(t => t.Id == transaction.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Splits);
        Assert.Single(result.Splits.First().Tags);
        Assert.Equal("Food", result.Splits.First().Tags.First().Name);
    }

    #endregion

    #region Transaction Operations

    /// <summary>
    /// Given a transaction
    /// When UpdateProperties is called
    /// Then properties should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task UpdateProperties_ExistingTransaction_UpdatesValues()
    {
        // Arrange
        var transaction = CreateTransaction(_instrumentId, -100m, "Test");
        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        transaction.UpdateProperties("Important note", true);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updated = await _context.Set<Transaction>().FindAsync([transaction.Id], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Important note", updated!.Notes);
        Assert.True(updated.ExcludeFromReporting);
    }

    /// <summary>
    /// Given a transaction with default split
    /// When EnsureMinimumSplit is called on transaction with splits
    /// Then no additional split is created
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task EnsureMinimumSplit_ExistingSplit_DoesNotAddAnother()
    {
        // Arrange
        var transaction = CreateTransaction(_instrumentId, -100m, "Test");
        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var initialSplitCount = transaction.Splits.Count;

        // Act
        transaction.EnsureMinimumSplit();

        // Assert
        Assert.Equal(initialSplitCount, transaction.Splits.Count);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given an existing transaction
    /// When removed from context
    /// Then the transaction should be deleted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingTransaction_RemovesTransaction()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction(transactionId)
        {
            AccountId = _instrumentId,
            Amount = -100m,
            Description = "To Delete",
            TransactionTime = DateTime.UtcNow,
            TransactionType = TransactionType.Debit,
            Source = "Test",
        };
        transaction.EnsureMinimumSplit();
        _context.Set<Transaction>().Add(transaction);
        await _context.SaveChangesAsync(true, TestContext.Current.CancellationToken);

        // Act
        _context.Set<Transaction>().Remove(transaction);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deleted = await _context.Set<Transaction>().FindAsync([transactionId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deleted);
    }

    #endregion

    private static Transaction CreateTransaction(Guid instrumentId, decimal amount, string description, Guid? institutionAccountId = null, DateTime? transactionTime = null)
    {
        var transaction = new Transaction(Guid.NewGuid())
        {
            AccountId = instrumentId,
            InstitutionAccountId = institutionAccountId,
            Amount = amount,
            Description = description,
            TransactionTime = transactionTime ?? DateTime.UtcNow,
            TransactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit,
            Source = "Test",
        };

        transaction.EnsureMinimumSplit();

        return transaction;
    }
}
