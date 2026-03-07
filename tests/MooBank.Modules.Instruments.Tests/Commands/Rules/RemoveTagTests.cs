#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class RemoveTagTests
{
    private readonly TestMocks _mocks;

    public RemoveTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesTagFromRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tagId = 10;
        var tag = TestEntities.CreateTag(id: tagId, name: "ToRemove");
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, tags: [tag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(rule.Tags);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
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

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

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

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, nonExistentRuleId, tagId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TagNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var nonExistentTagId = 999;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Existing");
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, tags: [existingTag]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, ruleId, nonExistentTagId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleTagsOnRule_RemovesOnlySpecified()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 1;
        var tag1 = TestEntities.CreateTag(id: 1, name: "Keep");
        var tag2 = TestEntities.CreateTag(id: 2, name: "Remove");
        var tag3 = TestEntities.CreateTag(id: 3, name: "AlsoKeep");
        var rule = TestEntities.CreateRule(id: ruleId, instrumentId: instrumentId, tags: [tag1, tag2, tag3]);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, ruleId, 2);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, rule.Tags.Count);
        Assert.Contains(rule.Tags, t => t.Id == 1);
        Assert.Contains(rule.Tags, t => t.Id == 3);
        Assert.DoesNotContain(rule.Tags, t => t.Id == 2);
    }

    [Fact]
    public async Task Handle_RemoveLastTag_LeavesRuleWithNoTags()
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

        var handler = new RemoveTagHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new RemoveTag(instrumentId, ruleId, tagId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(rule.Tags);
        // Rule still exists (just without tags)
        Assert.Single(instrument.Rules);
    }
}
