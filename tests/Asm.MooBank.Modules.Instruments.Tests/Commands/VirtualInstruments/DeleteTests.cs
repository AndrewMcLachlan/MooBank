#nullable enable
using Asm.Domain;
using Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.VirtualInstruments;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingVirtualInstrument_DeletesFromRepository()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(id: virtualInstrumentId);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, virtualInstrumentId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.InstrumentRepositoryMock.Verify(r => r.Delete(virtualInstrumentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingVirtualInstrument_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var virtualInstrumentId = Guid.NewGuid();
        var virtualInstrument = TestEntities.CreateVirtualInstrument(id: virtualInstrumentId);
        var instrument = TestEntities.CreateInstrument(
            id: instrumentId,
            virtualInstruments: [virtualInstrument]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, virtualInstrumentId);

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

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, nonExistentId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_VirtualInstrumentNotFound_DoesNotCallDelete()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, nonExistentId);

        // Act
        try
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _mocks.InstrumentRepositoryMock.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleVirtualInstruments_DeletesCorrectOne()
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

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<ISpecification<Domain.Entities.Instrument.Instrument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, virtualInstrumentId1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.InstrumentRepositoryMock.Verify(r => r.Delete(virtualInstrumentId1), Times.Once);
        _mocks.InstrumentRepositoryMock.Verify(r => r.Delete(virtualInstrumentId2), Times.Never);
    }
}
