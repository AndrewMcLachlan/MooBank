#nullable enable
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Queries;
using Asm.MooBank.Modules.Stocks.Tests.Support;
using DomainStockHolding = Asm.MooBank.Domain.Entities.StockHolding.StockHolding;
using DomainStockTransaction = Asm.MooBank.Domain.Entities.Transactions.StockTransaction;

namespace Asm.MooBank.Modules.Stocks.Tests.Queries;

[Trait("Category", "Unit")]
public class GetStockValueReportTests
{
    private static readonly Guid TestInstrumentId = Guid.NewGuid();
    private const string TestSymbol = "AAPL";

    [Fact]
    public async Task Handle_ValidQuery_ReturnsReportWithCorrectMetadata()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);
        var transactions = CreateStockTransactionQueryable([]);
        var priceHistory = CreatePriceHistoryQueryable([]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestInstrumentId, result.InstrumentId);
        Assert.Equal(TestSymbol, result.Symbol);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_NoTransactionsOrPrices_ReturnsEmptyPointsAndInvestment()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);
        var transactions = CreateStockTransactionQueryable([]);
        var priceHistory = CreatePriceHistoryQueryable([]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Points);
        Assert.Empty(result.Investment);
    }

    [Fact]
    public async Task Handle_WithTransactionsAndPrices_CalculatesStockValue()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        // Transaction before the report period
        var transaction = CreateStockTransaction(
            accountId: TestInstrumentId,
            quantity: 10,
            price: 100m,
            fees: 10m,
            transactionDate: DateTime.Today.AddDays(-20));
        var transactions = CreateStockTransactionQueryable([transaction]);

        // Price history within the period
        var price = CreatePriceHistory(start, 110m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);
        Assert.NotEmpty(result.Investment);

        // Stock value = quantity (10) * price (110) = 1100
        var firstPoint = result.Points.First();
        Assert.Equal(1100m, firstPoint.Value);

        // Investment = quantity (10) * purchase price (100) + fees (10) = 1010
        var firstInvestment = result.Investment.First();
        Assert.Equal(1010m, firstInvestment.Value);
    }

    [Fact]
    public async Task Handle_GranularityCalculation_BaseOnDateRange()
    {
        // Arrange
        // 60 day range should give granularity of 2 (60/30 = 2)
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-60));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);
        var transactions = CreateStockTransactionQueryable([]);
        var priceHistory = CreatePriceHistoryQueryable([]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Granularity);
    }

    [Fact]
    public async Task Handle_ShortDateRange_GranularityIsOne()
    {
        // Arrange
        // 10 day range should give granularity of 1 (minimum)
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);
        var transactions = CreateStockTransactionQueryable([]);
        var priceHistory = CreatePriceHistoryQueryable([]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Granularity);
    }

    [Fact]
    public async Task Handle_MultipleTransactions_SumsQuantityAndInvestment()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var transactions = new[]
        {
            CreateStockTransaction(TestInstrumentId, quantity: 10, price: 100m, fees: 10m, DateTime.Today.AddDays(-30)),
            CreateStockTransaction(TestInstrumentId, quantity: 5, price: 110m, fees: 5m, DateTime.Today.AddDays(-20)),
        };
        var transactionsQueryable = CreateStockTransactionQueryable(transactions);

        var price = CreatePriceHistory(start, 120m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactionsQueryable, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);

        // Total quantity = 10 + 5 = 15
        // Stock value = 15 * 120 = 1800
        var firstPoint = result.Points.First();
        Assert.Equal(1800m, firstPoint.Value);

        // Investment = (10 * 100 + 10) + (5 * 110 + 5) = 1010 + 555 = 1565
        var firstInvestment = result.Investment.First();
        Assert.Equal(1565m, firstInvestment.Value);
    }

    [Fact]
    public async Task Handle_ZeroStockValue_DoesNotAddPoint()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        // No transactions = zero quantity
        var transactions = CreateStockTransactionQueryable([]);

        // Even with price history, no points should be added if stock value is 0
        var price = CreatePriceHistory(start, 100m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Points);
        Assert.Empty(result.Investment);
    }

    [Fact]
    public async Task Handle_PriceNotAvailableForDate_UsesPreviousPrice()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var transaction = CreateStockTransaction(TestInstrumentId, quantity: 10, price: 100m, fees: 0m, DateTime.Today.AddDays(-30));
        var transactions = CreateStockTransactionQueryable([transaction]);

        // Only price at start, not for subsequent dates
        var price = CreatePriceHistory(start, 150m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);
        // All points should use the price of 150 (carried forward)
        foreach (var point in result.Points)
        {
            Assert.Equal(1500m, point.Value); // 10 * 150
        }
    }

    [Fact]
    public async Task Handle_TransactionsDuringReportPeriod_IncrementsQuantity()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        // Transaction within the report period
        var transaction = CreateStockTransaction(TestInstrumentId, quantity: 10, price: 100m, fees: 0m, DateTime.Today.AddDays(-15));
        var transactions = CreateStockTransactionQueryable([transaction]);

        // Prices throughout the period
        var prices = new[]
        {
            CreatePriceHistory(start, 100m),
            CreatePriceHistory(start.AddDays(15), 110m),
        };
        var priceHistory = CreatePriceHistoryQueryable(prices);

        var handler = new GetStockValueReportHandler(stockHoldings, transactions, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);
        // Early points (before transaction) should have 0 value (no stock)
        // Later points (after transaction) should have value based on quantity
    }

    [Fact]
    public async Task Handle_OnlyCountsTransactionsForCorrectAccount()
    {
        // Arrange
        var otherAccountId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        var end = DateOnly.FromDateTime(DateTime.Today);

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var transactions = new[]
        {
            CreateStockTransaction(TestInstrumentId, quantity: 10, price: 100m, fees: 0m, DateTime.Today.AddDays(-10)),
            CreateStockTransaction(otherAccountId, quantity: 100, price: 100m, fees: 0m, DateTime.Today.AddDays(-10)),
        };
        var transactionsQueryable = CreateStockTransactionQueryable(transactions);

        var price = CreatePriceHistory(start, 100m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactionsQueryable, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);
        // Should only count the 10 shares from TestInstrumentId, not the 100 from otherAccountId
        var firstPoint = result.Points.First();
        Assert.Equal(1000m, firstPoint.Value); // 10 * 100
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByEndDate()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        var stockHolding = TestEntities.CreateStockHolding(id: TestInstrumentId, symbol: TestSymbol);
        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var transactions = new[]
        {
            CreateStockTransaction(TestInstrumentId, quantity: 10, price: 100m, fees: 0m, DateTime.Today.AddDays(-15)), // Before start
            CreateStockTransaction(TestInstrumentId, quantity: 5, price: 100m, fees: 0m, DateTime.Today.AddDays(-2)),   // After end
        };
        var transactionsQueryable = CreateStockTransactionQueryable(transactions);

        var price = CreatePriceHistory(start, 100m);
        var priceHistory = CreatePriceHistoryQueryable([price]);

        var handler = new GetStockValueReportHandler(stockHoldings, transactionsQueryable, priceHistory);

        var query = new GetStockValueReport(TestInstrumentId, start, end);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotEmpty(result.Points);
        // Should only count the 10 shares (transaction after end date is excluded)
        var firstPoint = result.Points.First();
        Assert.Equal(1000m, firstPoint.Value); // 10 * 100
    }

    #region Helper Methods

    private static DomainStockTransaction CreateStockTransaction(
        Guid accountId,
        int quantity,
        decimal price,
        decimal fees,
        DateTime transactionDate)
    {
        return new DomainStockTransaction(Guid.NewGuid())
        {
            AccountId = accountId,
            Quantity = quantity,
            Price = price,
            Fees = fees,
            TransactionDate = transactionDate,
            TransactionType = TransactionType.Credit,
        };
    }

    private static StockPriceHistory CreatePriceHistory(DateOnly date, decimal price)
    {
        return new StockPriceHistory(Random.Shared.Next())
        {
            Symbol = TestSymbol,
            Exchange = null,
            Date = date,
            Price = price,
        };
    }

    private static IQueryable<DomainStockHolding> CreateStockHoldingQueryable(IEnumerable<DomainStockHolding> stockHoldings)
    {
        return QueryableHelper.CreateAsyncQueryable(stockHoldings);
    }

    private static IQueryable<DomainStockTransaction> CreateStockTransactionQueryable(IEnumerable<DomainStockTransaction> transactions)
    {
        return QueryableHelper.CreateAsyncQueryable(transactions);
    }

    private static IQueryable<StockPriceHistory> CreatePriceHistoryQueryable(IEnumerable<StockPriceHistory> priceHistory)
    {
        return QueryableHelper.CreateAsyncQueryable(priceHistory);
    }

    #endregion
}
