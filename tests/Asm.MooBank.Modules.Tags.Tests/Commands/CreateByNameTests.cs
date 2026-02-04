#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateByNameTests
{
    private readonly TestMocks _mocks;

    public CreateByNameTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTagWithName()
    {
        // Arrange
        var tagName = "New Tag";

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName(tagName, []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tagName, result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsUserFamilyId()
    {
        // Arrange
        DomainTag? addedTag = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(t => addedTag = t);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Test Tag", []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(addedTag);
        Assert.Equal(_mocks.User.FamilyId, addedTag.FamilyId);
    }

    [Fact]
    public async Task Handle_WithParentTags_AssociatesParentTags()
    {
        // Arrange
        var parentTag1 = TestEntities.CreateTag(id: 1, name: "Parent1");
        var parentTag2 = TestEntities.CreateTag(id: 2, name: "Parent2");

        DomainTag? addedTag = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), It.IsAny<CancellationToken>()))
            .ReturnsAsync([parentTag1, parentTag2]);
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(t => addedTag = t);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Child Tag", [1, 2]);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(addedTag);
        Assert.Equal(2, addedTag.Tags.Count);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsTagToRepository()
    {
        // Arrange
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Test Tag", []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.TagRepositoryMock.Verify(r => r.Add(It.IsAny<DomainTag>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Test Tag", []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewTag_HasDefaultSettings()
    {
        // Arrange
        DomainTag? addedTag = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(t => addedTag = t);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Test Tag", []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(addedTag);
        Assert.False(addedTag.Settings.ApplySmoothing);
        Assert.False(addedTag.Settings.ExcludeFromReporting);
    }

    [Fact]
    public async Task Handle_EmptyParentTags_CreatesTagWithNoParents()
    {
        // Arrange
        DomainTag? addedTag = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(t => addedTag = t);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Standalone Tag", []);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(addedTag);
        Assert.Empty(addedTag.Tags);
    }

    [Fact]
    public async Task Handle_ReturnsTagModel()
    {
        // Arrange
        _mocks.TagRepositoryMock
            .Setup(r => r.Get(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateByNameHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var command = new CreateByName("Test Tag", []);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MooBank.Models.Tag>(result);
    }
}
