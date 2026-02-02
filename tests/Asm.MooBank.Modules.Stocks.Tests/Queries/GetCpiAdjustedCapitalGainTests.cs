#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Queries;
using Asm.MooBank.Modules.Stocks.Tests.Support;

namespace Asm.MooBank.Modules.Stocks.Tests.Queries;

[Trait("Category", "Unit")]
public class GetCpiAdjustedCapitalGainTests
{
    private readonly TestMocks _mocks;

    public GetCpiAdjustedCapitalGainTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsAdjustedCapitalGain()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var currentValue = 1500m;
        var transaction = TestEntities.CreateStockTransaction(
            accountId: instrumentId,
            quantity: 10,
            price: 100m,
            transactionType: TransactionType.Credit);

        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            transactions: [transaction]);

        // Set CurrentValue
        typeof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding)
            .GetProperty(nameof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding.CurrentValue))!
            .SetValue(stockHolding, currentValue);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        // Default CPI service returns same value (no adjustment)
        var handler = new GetCpiAdjustedCapitalGainHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetCpiAdjustedCapitalGain(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // CurrentValue (1500) - Investment (10 * 100 = 1000) = 500
        Assert.Equal(500m, result);
    }

    [Fact]
    public async Task Handle_MultipleTransactions_SumsAdjustedValues()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var currentValue = 2000m;

        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 10, price: 100m, transactionType: TransactionType.Credit),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 5, price: 110m, transactionType: TransactionType.Credit),
        };

        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            transactions: transactions);

        typeof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding)
            .GetProperty(nameof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding.CurrentValue))!
            .SetValue(stockHolding, currentValue);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        // No adjustment for simplicity
        _mocks.CpiServiceMock
            .Setup(c => c.CalculateAdjustedValue(It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal value, DateTime _, CancellationToken _) => value);

        var handler = new GetCpiAdjustedCapitalGainHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetCpiAdjustedCapitalGain(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // CurrentValue (2000) - TotalInvestment (10*100 + 5*110 = 1550) = 450
        Assert.Equal(450m, result);
    }

    [Fact]
    public async Task Handle_DebitTransaction_SubtractsFromTotal()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var currentValue = 700m;

        var transactions = new[]
        {
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 10, price: 100m, transactionType: TransactionType.Credit),
            TestEntities.CreateStockTransaction(accountId: instrumentId, quantity: 3, price: 120m, transactionType: TransactionType.Debit),
        };

        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            transactions: transactions);

        typeof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding)
            .GetProperty(nameof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding.CurrentValue))!
            .SetValue(stockHolding, currentValue);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        _mocks.CpiServiceMock
            .Setup(c => c.CalculateAdjustedValue(It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal value, DateTime _, CancellationToken _) => value);

        var handler = new GetCpiAdjustedCapitalGainHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetCpiAdjustedCapitalGain(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        // CurrentValue (700) - AdjustedInvestment (10*100 - 3*120 = 1000 - 360 = 640) = 60
        Assert.Equal(60m, result);
    }

    [Fact]
    public async Task Handle_NonExistentInstrument_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHoldings = TestEntities.CreateStockHoldingQueryable();

        var handler = new GetCpiAdjustedCapitalGainHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetCpiAdjustedCapitalGain(instrumentId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NoTransactions_ReturnsCurrentValue()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var currentValue = 1000m;

        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        typeof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding)
            .GetProperty(nameof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding.CurrentValue))!
            .SetValue(stockHolding, currentValue);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        var handler = new GetCpiAdjustedCapitalGainHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetCpiAdjustedCapitalGain(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(currentValue, result);
    }
}
