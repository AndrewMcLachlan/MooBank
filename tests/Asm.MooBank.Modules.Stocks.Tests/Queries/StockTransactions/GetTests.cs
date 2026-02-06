#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Queries.StockTransactions;
using Asm.MooBank.Modules.Stocks.Tests.Support;

namespace Asm.MooBank.Modules.Stocks.Tests.Queries.StockTransactions;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 10),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 5),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
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
        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Results.Count());
    }

    [Fact]
    public async Task Handle_FilterByInstrumentId_ReturnsOnlyMatchingTransactions()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 10),
            TestEntities.CreateStockTransaction(accountId: otherId, quantity: 5),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 3),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
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
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task Handle_StartDateFilter_ReturnsTransactionsAfterDate()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var oldDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: oldDate),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: newDate),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            Start = new DateTime(2024, 3, 1),
            PageSize = 10,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_EndDateFilter_ReturnsTransactionsBeforeDate()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var oldDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: oldDate),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: newDate),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            End = new DateTime(2024, 3, 1),
            PageSize = 10,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_DateRangeFilter_ReturnsTransactionsInRange()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            Start = new DateTime(2024, 2, 1),
            End = new DateTime(2024, 6, 1),
            PageSize = 10,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Total);
    }

    [Fact]
    public async Task Handle_NoSortField_SortsByTransactionDateAscending()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortDirection = SortDirection.Ascending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var results = result.Results.ToList();
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].TransactionDate);
        Assert.Equal(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), results[1].TransactionDate);
        Assert.Equal(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), results[2].TransactionDate);
    }

    [Fact]
    public async Task Handle_NoSortField_Descending_SortsByTransactionDateDescending()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            TestEntities.CreateStockTransaction(accountId: instrumentId, transactionDate: new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortDirection = SortDirection.Descending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var results = result.Results.ToList();
        Assert.Equal(new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc), results[0].TransactionDate);
        Assert.Equal(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), results[1].TransactionDate);
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[2].TransactionDate);
    }

    [Fact]
    public async Task Handle_SortByPrice_Ascending_SortsCorrectly()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 150m),
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 50m),
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 100m),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortField = "Price",
            SortDirection = SortDirection.Ascending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var results = result.Results.ToList();
        Assert.Equal(50m, results[0].Price);
        Assert.Equal(100m, results[1].Price);
        Assert.Equal(150m, results[2].Price);
    }

    [Fact]
    public async Task Handle_SortByPrice_Descending_SortsCorrectly()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 150m),
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 50m),
            TestEntities.CreateStockTransaction(accountId: instrumentId, price: 100m),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortField = "Price",
            SortDirection = SortDirection.Descending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var results = result.Results.ToList();
        Assert.Equal(150m, results[0].Price);
        Assert.Equal(100m, results[1].Price);
        Assert.Equal(50m, results[2].Price);
    }

    [Fact]
    public async Task Handle_SortByQuantity_SortsCorrectly()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 30),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 10),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 20),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortField = "Quantity",
            SortDirection = SortDirection.Ascending,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var results = result.Results.ToList();
        Assert.Equal(10, results[0].Quantity);
        Assert.Equal(20, results[1].Quantity);
        Assert.Equal(30, results[2].Quantity);
    }

    [Fact]
    public async Task Handle_UnknownSortField_ThrowsArgumentException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            PageSize = 10,
            PageNumber = 1,
            SortField = "NonExistentField",
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = Enumerable.Range(1, 25)
            .Select(i => TestEntities.CreateStockTransaction(
                accountId: instrumentId,
                quantity: i,
                transactionDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(i)))
            .ToArray();

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
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
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var queryable = TestEntities.CreateStockTransactionQueryable([]);
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
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task Handle_NoDateFilters_ReturnsAllTransactions()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId),
            TestEntities.CreateStockTransaction(accountId: instrumentId),
            TestEntities.CreateStockTransaction(accountId: instrumentId),
        };

        var queryable = TestEntities.CreateStockTransactionQueryable(transactions);
        var handler = new GetHandler(queryable);

        var query = new Get
        {
            InstrumentId = instrumentId,
            Start = null,
            End = null,
            PageSize = 10,
            PageNumber = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Total);
    }
}
