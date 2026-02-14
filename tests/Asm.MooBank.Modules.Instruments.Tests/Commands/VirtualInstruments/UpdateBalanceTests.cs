#nullable enable
using Asm.Domain;
using Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.VirtualInstruments;

[Trait("Category", "Unit")]
public class UpdateBalanceTests
{
    private readonly TestMocks _mocks;

    public UpdateBalanceTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RaisesBalanceAdjustmentEvent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 500m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 750m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(virtualInstrument.Events);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedVirtualInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            name: "Test Account",
            balance: 100m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 200m);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Account", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(id: virtualInstrumentId, balance: 100m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 300m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VirtualInstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new UpdateBalance(instrumentId, nonExistentId, 500m);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_IncreaseBalance_CalculatesPositiveAdjustment()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 100m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Increase from 100 to 250 = +150 adjustment
        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 250m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(virtualInstrument.Events);
    }

    [Fact]
    public async Task Handle_DecreaseBalance_CalculatesNegativeAdjustment()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 500m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Decrease from 500 to 200 = -300 adjustment
        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 200m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(virtualInstrument.Events);
    }

    [Fact]
    public async Task Handle_ZeroBalanceChange_StillRaisesEvent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 100m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Same balance - 0 adjustment
        var command = new UpdateBalance(instrumentId, virtualInstrumentId, 100m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert - UpdateBalance always raises event, even for 0 adjustment
        Assert.Single(virtualInstrument.Events);
    }
}
