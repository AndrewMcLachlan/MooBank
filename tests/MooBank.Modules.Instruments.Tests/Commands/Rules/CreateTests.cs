#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsRuleToInstrument()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, "WOOLWORTHS", "Grocery store", []);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(instrument.Rules);
        Assert.Equal("WOOLWORTHS", instrument.Rules.First().Contains);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, "WOOLWORTHS", "Grocery store", []);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("WOOLWORTHS", result.Contains);
        Assert.Equal("Grocery store", result.Description);
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

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, "WOOLWORTHS", "Grocery store", []);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTags_AssociatesTagsWithRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);
        var domainTag1 = TestEntities.CreateTag(id: 1, name: "Groceries");
        var domainTag2 = TestEntities.CreateTag(id: 2, name: "Food");

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([domainTag1, domainTag2]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var tags = new[] { TestEntities.CreateTagModel(id: 1), TestEntities.CreateTagModel(id: 2) };
        var command = new Create(instrumentId, "WOOLWORTHS", "Grocery store", tags);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, instrument.Rules.First().Tags.Count);
    }

    [Fact]
    public async Task Handle_WithTags_FetchesTagsFromRepository()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var tags = new[] { TestEntities.CreateTagModel(id: 1), TestEntities.CreateTagModel(id: 2) };
        var command = new Create(instrumentId, "WOOLWORTHS", "Grocery store", tags);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.TagRepositoryMock.Verify(r => r.Get(It.Is<IEnumerable<int>>(ids => ids.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullDescription_SetsDescriptionToNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, "WOOLWORTHS", null, []);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.Description);
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
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, "WOOLWORTHS", "Description", []);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
