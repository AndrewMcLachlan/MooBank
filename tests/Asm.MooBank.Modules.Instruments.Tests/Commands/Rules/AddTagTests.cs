#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class AddTagTests
{
    private readonly TestMocks _mocks;

    public AddTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsTagToRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId);
        var tag = TestEntities.CreateTag(id: tagId, name: "Groceries");
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(rule.Tags);
        Assert.Equal(tagId, rule.Tags.First().Id);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, contains: "COLES");
        var tag = TestEntities.CreateTag(id: tagId, name: "Shopping");
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, ruleId, tagId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("COLES", result.Contains);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId);
        var tag = TestEntities.CreateTag(id: tagId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RuleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentRuleId = 999;
        var tagId = 10;
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, nonExistentRuleId, tagId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_TagAlreadyExists_ReturnsWithoutAdding()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var tag = TestEntities.CreateTag(id: tagId, name: "Existing");
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, tags: [tag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, ruleId, tagId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(rule.Tags);
        _mocks.TagRepositoryMock.Verify(r => r.Get(tagId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TagAlreadyExists_DoesNotSaveChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var tag = TestEntities.CreateTag(id: tagId);
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, tags: [tag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new AddTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AddMultipleTags_AllTagsAdded()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId);
        var tag1 = TestEntities.CreateTag(id: 1, name: "Tag1");
        var tag2 = TestEntities.CreateTag(id: 2, name: "Tag2");
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag1);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag2);

        var handler = new AddTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        // Act
        await handler.Handle(new AddTag(instrumentId, ruleId, 1), CancellationToken.None);
        await handler.Handle(new AddTag(instrumentId, ruleId, 2), CancellationToken.None);

        // Assert
        Assert.Equal(2, rule.Tags.Count);
    }
}
