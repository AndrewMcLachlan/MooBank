#nullable enable
using Asm.MooBank.Modules.Stocks.Queries;
using Asm.MooBank.Modules.Stocks.Tests.Support;

namespace Asm.MooBank.Modules.Stocks.Tests.Queries;

[Trait("Category", "Unit")]
public class GetStockHoldingReportTests
{
    private readonly TestMocks _mocks;

    public GetStockHoldingReportTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsStockHoldingReport()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            currentPrice: 150m,
            quantity: 10,
            gainLoss: 500m);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        var handler = new GetStockHoldingReportHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetStockHoldingReport(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(instrumentId, result.AccountId);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCurrentValue()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var expectedCurrentValue = 1500m;
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            currentPrice: 150m,
            quantity: 10);

        // Set CurrentValue via reflection since it's computed
        typeof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding)
            .GetProperty(nameof(Asm.MooBank.Domain.Entities.StockHolding.StockHolding.CurrentValue))!
            .SetValue(stockHolding, expectedCurrentValue);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        var handler = new GetStockHoldingReportHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetStockHoldingReport(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedCurrentValue, result.CurrentValue);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsProfitLoss()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var expectedGainLoss = 500m;
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            gainLoss: expectedGainLoss);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        var handler = new GetStockHoldingReportHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetStockHoldingReport(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedGainLoss, result.ProfitLoss);
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsCpiServiceForAdjustedProfitLoss()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var gainLoss = 500m;
        var adjustedValue = 550m;
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            gainLoss: gainLoss);

        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);

        _mocks.CpiServiceMock
            .Setup(c => c.CalculateAdjustedValue(gainLoss, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustedValue);

        var handler = new GetStockHoldingReportHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetStockHoldingReport(instrumentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(adjustedValue, result.AdjustedProfitLoss);
    }

    [Fact]
    public async Task Handle_NonExistentInstrument_ThrowsException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHoldings = TestEntities.CreateStockHoldingQueryable();

        var handler = new GetStockHoldingReportHandler(stockHoldings, _mocks.CpiServiceMock.Object);

        var query = new GetStockHoldingReport(instrumentId);

        // Act & Assert
        // The async query provider wraps the exception in TargetInvocationException
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(query, CancellationToken.None).AsTask());
        Assert.True(
            exception is InvalidOperationException ||
            (exception.InnerException is InvalidOperationException),
            "Expected InvalidOperationException or wrapped InvalidOperationException");
    }
}
