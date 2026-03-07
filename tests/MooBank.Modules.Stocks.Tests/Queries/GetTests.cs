#nullable enable
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Stocks.Queries;
using Asm.MooBank.Modules.Stocks.Tests.Support;
using DomainStockHolding = Asm.MooBank.Domain.Entities.StockHolding.StockHolding;

namespace Asm.MooBank.Modules.Stocks.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsStockHolding()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            name: "Apple Inc",
            symbol: "AAPL",
            currentPrice: 150m,
            quantity: 10);

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(instrumentId, result.Id);
        Assert.Equal("Apple Inc", result.Name);
        Assert.Equal("AAPL", result.Symbol);
    }

    [Fact]
    public async Task Handle_NonExistentInstrument_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var stockHoldings = CreateStockHoldingQueryable([]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(nonExistentId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ReturnsCorrectStockValues()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            currentPrice: 100m,
            quantity: 50,
            gainLoss: 500m);

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(100m, result.CurrentPrice);
        Assert.Equal(50, result.Quantity);
        Assert.Equal(500m, result.GainLoss);
    }

    [Fact]
    public async Task Handle_ConvertsCurrencyForLocalBalance()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId, currentPrice: 100m, quantity: 10);

        // Set up currency converter to return a different value
        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns(1500m); // Converted value

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1500m, result.CurrentBalanceLocalCurrency);
        _mocks.CurrencyConverterMock.Verify(c => c.Convert(It.IsAny<decimal>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOwners_IncludesOwnerRelationships()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var userId = _mocks.User.Id;

        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        // Add an owner to the stock holding
        stockHolding.Owners.Add(new InstrumentOwner
        {
            InstrumentId = instrumentId,
            UserId = userId,
        });

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(instrumentId, result.Id);
    }

    [Fact]
    public async Task Handle_FindsCorrectInstrumentAmongMultiple()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var targetHolding = TestEntities.CreateStockHolding(id: targetId, name: "Target", symbol: "TGT");
        var otherHolding = TestEntities.CreateStockHolding(id: otherId, name: "Other", symbol: "OTH");

        var stockHoldings = CreateStockHoldingQueryable([targetHolding, otherHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target", result.Name);
        Assert.Equal("TGT", result.Symbol);
    }

    [Fact]
    public async Task Handle_ReturnsInstrumentType()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Shares", result.InstrumentType);
    }

    [Fact]
    public async Task Handle_SetsGroupIdFromUserOwnership()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();

        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        // Add an owner with a group
        var group = new Asm.MooBank.Domain.Entities.Group.Group(groupId)
        {
            Name = "Test Group",
            OwnerId = userId,
        };

        stockHolding.Owners.Add(new InstrumentOwner
        {
            InstrumentId = instrumentId,
            UserId = userId,
            GroupId = groupId,
            Group = group,
        });

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.GroupId);
    }

    [Fact]
    public async Task Handle_NoGroupId_ReturnsNullGroupId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        // No owners or groups added

        var stockHoldings = CreateStockHoldingQueryable([stockHolding]);

        var handler = new GetHandler(stockHoldings, _mocks.User, _mocks.CurrencyConverterMock.Object);

        var query = new Get(instrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.GroupId);
    }

    #region Helper Methods

    private static IQueryable<DomainStockHolding> CreateStockHoldingQueryable(IEnumerable<DomainStockHolding> stockHoldings)
    {
        return QueryableHelper.CreateAsyncQueryable(stockHoldings);
    }

    #endregion
}
