#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Asm.MooBank.Modules.Transactions.Tests.Support;

namespace Asm.MooBank.Modules.Transactions.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_WithTransactions_ReturnsPagedResult()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 5)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.Total);
        Assert.Equal(5, result.Results.Count());
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Results.Count());
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsNextItems()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 2,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(10, result.Results.Count());
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 3,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(25, result.Total);
        Assert.Equal(5, result.Results.Count());
    }

    [Fact]
    public async Task Handle_NoTransactions_ReturnsEmptyResult()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var queryable = TestEntities.CreateTransactionQueryable([]);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_FiltersById_ReturnsOnlyMatchingTransactions()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var otherInstrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateTransaction(accountId: instrumentId, description: "Matching 1"),
            TestEntities.CreateTransaction(accountId: instrumentId, description: "Matching 2"),
            TestEntities.CreateTransaction(accountId: otherInstrumentId, description: "Other"),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.All(result.Results, t => Assert.Equal(instrumentId, t.AccountId));
    }

    [Fact]
    public async Task Handle_WithDateFilter_FiltersTransactions()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateTransaction(accountId: instrumentId, transactionTime: new DateTime(2024, 1, 15)),
            TestEntities.CreateTransaction(accountId: instrumentId, transactionTime: new DateTime(2024, 2, 15)),
            TestEntities.CreateTransaction(accountId: instrumentId, transactionTime: new DateTime(2024, 3, 15)),
        };
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
            Start = new DateTime(2024, 2, 1),
            End = new DateTime(2024, 2, 28),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Total);
    }

    // Note: Text filter test removed as it uses EF Core's DbFunctions.Like which requires database integration

    // Note: AccountHolderName sort field mapping (MapSortFieldNames) requires User objects to be populated,
    // which is complex to set up in unit tests. The branch "AccountHolderName" => "User.FirstName"
    // should be covered by integration tests.

    [Fact]
    public async Task Handle_WithOtherSortField_DoesNotMapFieldName()
    {
        // Arrange - Test the default branch (non-AccountHolderName sort field)
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 3)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
            SortField = "TransactionTime", // This should NOT trigger the mapping branch
            SortDirection = SortDirection.Descending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Results.Count());
    }

    [Fact]
    public async Task Handle_WithNullSortField_DoesNotMapFieldName()
    {
        // Arrange - Test with null sort field (default path)
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 3)
            .Select(i => TestEntities.CreateTransaction(accountId: instrumentId, description: $"Transaction {i}"))
            .ToArray();
        var queryable = TestEntities.CreateTransactionQueryable(transactions);

        var handler = new GetHandler(queryable);
        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 20,
            PageNumber = 1,
            SortField = null,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Results.Count());
    }
}
