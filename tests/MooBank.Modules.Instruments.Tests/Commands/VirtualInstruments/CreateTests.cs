#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.VirtualInstruments;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsVirtualInstrumentToParent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var createModel = TestEntities.CreateVirtualInstrumentModel(name: "Savings Goal", openingBalance: 500m);
        var command = new Create(instrumentId, createModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(instrument.VirtualInstruments);
        Assert.Equal("Savings Goal", instrument.VirtualInstruments.First().Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedVirtualInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var createModel = TestEntities.CreateVirtualInstrumentModel(name: "Emergency Fund", description: "For emergencies");
        var command = new Create(instrumentId, createModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Emergency Fund", result.Name);
        Assert.Equal("For emergencies", result.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create(instrumentId, TestEntities.CreateVirtualInstrumentModel());

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_InheritsCurrencyFromParent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId, currency: "USD");

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create(instrumentId, TestEntities.CreateVirtualInstrumentModel());

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("USD", instrument.VirtualInstruments.First().Currency);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsController()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var createModel = TestEntities.CreateVirtualInstrumentModel(controller: Controller.Virtual);
        var command = new Create(instrumentId, createModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(Controller.Virtual, instrument.VirtualInstruments.First().Controller);
    }

    [Fact]
    public async Task Handle_NullDescription_AllowsNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        // Create model directly to ensure null description
        var createModel = new Instruments.Models.Virtual.CreateVirtualInstrument
        {
            Name = "No Description",
            Description = null,
        };
        var command = new Create(instrumentId, createModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task Handle_WithOpeningBalance_RaisesEvent()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var createModel = TestEntities.CreateVirtualInstrumentModel(openingBalance: 1000m);
        var command = new Create(instrumentId, createModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        // AddVirtualInstrument raises VirtualInstrumentAddedEvent on the parent instrument
        Assert.NotEmpty(instrument.Events);
    }

    [Fact]
    public async Task Handle_InstrumentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var createModel = TestEntities.CreateVirtualInstrumentModel(name: "Test");
        var command = new Create(instrumentId, createModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
