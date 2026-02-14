#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.VirtualAccounts;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.VirtualAccounts;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingVirtualInstrument_ReturnsVirtualInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            name: "Savings Goal");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, virtualInstrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Savings Goal", result.Name);
        Assert.Equal(virtualInstrumentId, result.Id);
    }

    [Fact]
    public async Task Handle_InstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var queryable = TestEntities.CreateInstrumentQueryable([]);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(nonExistentId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_VirtualInstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentVirtualId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, nonExistentVirtualId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleVirtualInstruments_ReturnsCorrectOne()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId1 = Guid.NewGuid();
        var virtualInstrumentId2 = Guid.NewGuid();
        var virtualInstrument1 = TestEntities.CreateVirtualInstrument(id: virtualInstrumentId1, name: "First");
        var virtualInstrument2 = TestEntities.CreateVirtualInstrument(id: virtualInstrumentId2, name: "Second");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument1, virtualInstrument2]);

        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, virtualInstrumentId2);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Second", result.Name);
        Assert.Equal(virtualInstrumentId2, result.Id);
    }

    [Fact]
    public async Task Handle_VirtualInstrument_ReturnsCorrectProperties()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            name: "Emergency Fund",
            description: "For emergencies",
            balance: 5000m,
            currency: "AUD");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, virtualInstrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Emergency Fund", result.Name);
        Assert.Equal("For emergencies", result.Description);
        Assert.Equal(5000m, result.CurrentBalance);
        Assert.Equal("AUD", result.Currency);
    }

    [Fact]
    public async Task Handle_UsesCurrencyConverter()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 100m,
            currency: "USD");
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m); // Converted amount

        var queryable = TestEntities.CreateInstrumentQueryable(instrument);

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, virtualInstrumentId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(100m, result.CurrentBalance);
        Assert.Equal(150m, result.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public async Task Handle_NonLogicalAccountInstrument_ThrowsInvalidOperationException()
    {
        // Arrange - StockHolding is not a LogicalAccount, so virtual accounts are not supported
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        var queryable = TestEntities.CreateStockHoldingQueryable(stockHolding)
            .Cast<Domain.Entities.Instrument.Instrument>();

        var handler = new GetHandler(queryable, _mocks.CurrencyConverterMock.Object);
        var query = new Get(instrumentId, Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
        Assert.Contains("Virtual accounts are only available for institution accounts", exception.Message);
    }
}
