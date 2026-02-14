#nullable enable
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Tests.Support;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;

namespace Asm.MooBank.Modules.Groups.Tests.Commands;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new DeleteHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Delete(groupId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.GroupRepositoryMock.Verify(r => r.Delete(group), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new DeleteHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Delete(groupId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksGroupPermission()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new DeleteHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Delete(groupId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(group), Times.Once);
    }

    [Fact]
    public async Task Handle_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, ownerId: Guid.NewGuid()); // Different owner

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(group))
            .Throws(new NotAuthorisedException());

        var handler = new DeleteHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Delete(groupId);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NoPermission_DoesNotDeleteGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, ownerId: Guid.NewGuid());

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(group))
            .Throws(new NotAuthorisedException());

        var handler = new DeleteHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Delete(groupId);

        // Act
        try
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        }
        catch (NotAuthorisedException)
        {
            // Expected
        }

        // Assert
        _mocks.GroupRepositoryMock.Verify(r => r.Delete(It.IsAny<DomainGroup>()), Times.Never);
    }
}
