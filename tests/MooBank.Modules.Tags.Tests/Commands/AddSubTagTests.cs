#nullable enable
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class AddSubTagTests
{
    private readonly TestMocks _mocks;

    public AddSubTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsSubTagAndReturnsTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var emptyRelationships = TestMocks.CreateEmptyTagRelationships();

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            emptyRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(parentTag.Tags, t => t.Id == 2);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var emptyRelationships = TestMocks.CreateEmptyTagRelationships();

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            emptyRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SameTagId_ThrowsExistsException()
    {
        // Arrange
        var emptyRelationships = TestMocks.CreateEmptyTagRelationships();

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            emptyRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ExistsException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_DifferentFamilies_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId1 = Guid.NewGuid();
        var familyId2 = Guid.NewGuid();
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId1);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId2);
        var emptyRelationships = TestMocks.CreateEmptyTagRelationships();

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId1))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            emptyRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_SubTagAlreadyExists_ThrowsExistsException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);

        // Relationship already exists
        var existingRelationships = new List<TagRelationship>
        {
            new() { Tag = childTag, ParentTag = parentTag }
        };

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            existingRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<ExistsException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_CircularRelationship_ThrowsExistsException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);

        // Child is already parent of the proposed parent (circular)
        var existingRelationships = new List<TagRelationship>
        {
            new() { Tag = parentTag, ParentTag = childTag }
        };

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            existingRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<ExistsException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var emptyRelationships = TestMocks.CreateEmptyTagRelationships();

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new AddSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            emptyRelationships,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new AddSubTag(1, 2);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertFamilyPermission(familyId), Times.Once);
    }
}
