#nullable enable
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Tests.Support;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;

namespace Asm.MooBank.Modules.Groups.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedGroup()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, name: "Old Name", ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name", description: "New Description", showTotal: true);
        var command = new Update(groupModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.True(result.ShowTotal);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, name: "Old Name", description: "Old Description", ownerId: _mocks.User.Id, showPosition: false);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name", description: "New Description", showTotal: true);
        var command = new Update(groupModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", existingGroup.Name);
        Assert.Equal("New Description", existingGroup.Description);
        Assert.True(existingGroup.ShowPosition);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name");
        var command = new Update(groupModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksGroupPermission()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, ownerId: _mocks.User.Id);

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name");
        var command = new Update(groupModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(existingGroup), Times.Once);
    }

    [Fact]
    public async Task Handle_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, ownerId: Guid.NewGuid()); // Different owner

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(existingGroup))
            .Throws(new NotAuthorisedException());

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name");
        var command = new Update(groupModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NoPermission_DoesNotSaveChanges()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingGroup = TestEntities.CreateGroup(id: groupId, ownerId: Guid.NewGuid());

        _mocks.GroupRepositoryMock
            .Setup(r => r.Get(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGroup);

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(existingGroup))
            .Throws(new NotAuthorisedException());

        var handler = new UpdateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var groupModel = TestEntities.CreateGroupModel(id: groupId, name: "New Name");
        var command = new Update(groupModel);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (NotAuthorisedException)
        {
            // Expected
        }

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
