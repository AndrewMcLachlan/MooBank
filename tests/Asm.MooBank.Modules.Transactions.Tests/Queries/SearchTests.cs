#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Asm.MooBank.Modules.Transactions.Tests.Support;

namespace Asm.MooBank.Modules.Transactions.Tests.Queries;

[Trait("Category", "Unit")]
public class SearchTests
{
    [Fact]
    public async Task Handle_MatchingTransactions_ReturnsResults()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_NoMatchingTags_ReturnsEmpty()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [99]); // Non-matching tag ID

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_DifferentTransactionType_ReturnsEmpty()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Credit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_TransactionBeforeStartDate_ExcludesTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 1, 15), // Before start date
                transactionType: TransactionType.Debit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_TransactionWithinGracePeriod_IncludesTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 2, 27), // 2 days before start (within 5 day grace)
                transactionType: TransactionType.Debit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_DifferentInstrument_ReturnsEmpty()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var otherInstrumentId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: otherInstrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit,
                tags: [tag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MultipleMatchingTags_ReturnsTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var tag1 = TestEntities.CreateTag(id: 1, name: "Groceries");
        var tag2 = TestEntities.CreateTag(id: 2, name: "Food");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit,
                tags: [tag1, tag2]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [2]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_MultipleSearchTags_ReturnsTransactionsWithAnyTag()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var groceriesTag = TestEntities.CreateTag(id: 1, name: "Groceries");
        var fuelTag = TestEntities.CreateTag(id: 2, name: "Fuel");
        var transactions = new[]
        {
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit,
                tags: [groceriesTag]),
            TestEntities.CreateTransaction(
                accountId: instrumentId,
                transactionTime: new DateTime(2024, 3, 16),
                transactionType: TransactionType.Debit,
                tags: [fuelTag]),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new SearchHandler(queryable);
        var query = new Search(instrumentId, new DateOnly(2024, 3, 1), TransactionType.Debit, [1, 2]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
    }
}
