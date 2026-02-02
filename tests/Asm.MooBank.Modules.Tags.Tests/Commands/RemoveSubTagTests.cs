#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class RemoveSubTagTests
{
    private readonly TestMocks _mocks;

    public RemoveSubTagTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesSubTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId, subTags: [childTag]);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new RemoveSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RemoveSubTag(1, 2);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(parentTag.Tags, t => t.Id == 2);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId, subTags: [childTag]);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new RemoveSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RemoveSubTag(1, 2);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SubTagNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var otherChildTag = TestEntities.CreateTag(id: 3, name: "Other Child", familyId: familyId);
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId, subTags: [otherChildTag]);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new RemoveSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RemoveSubTag(1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId, subTags: [childTag]);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new RemoveSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RemoveSubTag(1, 2);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertFamilyPermission(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_NoSubTags_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentTag);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(2, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new RemoveSubTagHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RemoveSubTag(1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }
}
