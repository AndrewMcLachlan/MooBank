#nullable enable
using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.VirtualInstruments;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesNameAndDescription()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            name: "Old Name",
            description: "Old Description",
            balance: 500m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update(instrumentId, virtualInstrumentId, "New Name", "New Description", 500m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", virtualInstrument.Name);
        Assert.Equal("New Description", virtualInstrument.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedVirtualInstrument()
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

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update(instrumentId, virtualInstrumentId, "Updated Name", "Updated Desc", 500m);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Desc", result.Description);
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

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update(instrumentId, virtualInstrumentId, "Name", "Desc", 100m);

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

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update(instrumentId, nonExistentId, "Name", "Desc", 0m);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    /// <summary>
    /// Given a virtual instrument with a balance of 1000
    /// When it is updated with a lower balance of 800
    /// Then a balance adjustment event is raised for -200 and the balance is not set directly.
    /// </summary>
    [Fact]
    public async Task Handle_BalanceDecrease_RaisesBalanceAdjustmentEvent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(
            id: virtualInstrumentId,
            balance: 1000m);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // CurrentBalance lower than existing balance triggers adjustment
        var command = new Update(instrumentId, virtualInstrumentId, "Name", "Desc", 800m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var domainEvent = Assert.IsType<BalanceAdjustmentEvent>(Assert.Single(virtualInstrument.Events));
        Assert.Equal(-200m, domainEvent.Amount);
        // The balance itself is a computed column applied via the adjustment transaction,
        // so the in-memory value is not set directly.
        Assert.Equal(1000m, virtualInstrument.Balance);
    }

    [Fact]
    public async Task Handle_BalanceUnchanged_NoEventRaised()
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

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Same balance - no adjustment needed
        var command = new Update(instrumentId, virtualInstrumentId, "Name", "Desc", 500m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(virtualInstrument.Events);
    }

    /// <summary>
    /// Given a virtual instrument with a balance of 500
    /// When it is updated with a higher balance of 800
    /// Then a balance adjustment event is raised for +300.
    /// </summary>
    [Fact]
    public async Task Handle_BalanceIncrease_RaisesBalanceAdjustmentEvent()
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

        var handler = new UpdateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Higher balance requested - amount = 800 - 500 = +300
        var command = new Update(instrumentId, virtualInstrumentId, "Name", "Desc", 800m);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var domainEvent = Assert.IsType<BalanceAdjustmentEvent>(Assert.Single(virtualInstrument.Events));
        Assert.Equal(300m, domainEvent.Amount);
    }
}
