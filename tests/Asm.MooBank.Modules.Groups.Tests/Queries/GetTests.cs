#nullable enable
using Asm.MooBank.Modules.Groups.Queries;
using Asm.MooBank.Modules.Groups.Tests.Support;

namespace Asm.MooBank.Modules.Groups.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingGroup_ReturnsGroup()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, name: "My Group", ownerId: userId);
        var queryable = TestEntities.CreateGroupQueryable(group);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(groupId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.Id);
        Assert.Equal("My Group", result.Name);
    }

    [Fact]
    public async Task Handle_MultipleGroups_ReturnsCorrectOne()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var targetId = Guid.NewGuid();
        var groups = new[]
        {
            TestEntities.CreateGroup(name: "Group 1", ownerId: userId),
            TestEntities.CreateGroup(id: targetId, name: "Target Group", ownerId: userId),
            TestEntities.CreateGroup(name: "Group 3", ownerId: userId),
        };
        var queryable = TestEntities.CreateGroupQueryable(groups);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target Group", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var group = TestEntities.CreateGroup(name: "Some Group", ownerId: userId);
        var queryable = TestEntities.CreateGroupQueryable(group);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_GroupFromDifferentOwner_ThrowsNotFoundException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, name: "Other User's Group", ownerId: otherUserId);
        var queryable = TestEntities.CreateGroupQueryable(group);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(groupId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyQueryable_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateGroupQueryable([]);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_GroupWithShowPosition_MapsToShowTotal()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(id: groupId, name: "Totals Group", ownerId: userId, showPosition: true);
        var queryable = TestEntities.CreateGroupQueryable(group);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(groupId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.ShowTotal);
    }
}
