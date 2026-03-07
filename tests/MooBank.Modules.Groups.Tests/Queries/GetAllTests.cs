#nullable enable
using Asm.MooBank.Modules.Groups.Queries;
using Asm.MooBank.Modules.Groups.Tests.Support;

namespace Asm.MooBank.Modules.Groups.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithGroups_ReturnsAllUserGroups()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groups = TestEntities.CreateSampleGroups(userId);
        var queryable = TestEntities.CreateGroupQueryable(groups);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_NoGroups_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateGroupQueryable([]);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FiltersToUserOwned()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var otherUserId = Guid.NewGuid();
        var groups = new[]
        {
            TestEntities.CreateGroup(name: "User Group 1", ownerId: userId),
            TestEntities.CreateGroup(name: "User Group 2", ownerId: userId),
            TestEntities.CreateGroup(name: "Other User Group", ownerId: otherUserId),
        };
        var queryable = TestEntities.CreateGroupQueryable(groups);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, g => Assert.StartsWith("User Group", g.Name));
    }

    [Fact]
    public async Task Handle_MapsAllProperties()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();
        var group = TestEntities.CreateGroup(
            id: groupId,
            name: "Test Group",
            description: "Test Description",
            ownerId: userId,
            showPosition: true);
        var queryable = TestEntities.CreateGroupQueryable(group);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultGroup = result.Single();
        Assert.Equal(groupId, resultGroup.Id);
        Assert.Equal("Test Group", resultGroup.Name);
        Assert.Equal("Test Description", resultGroup.Description);
        Assert.True(resultGroup.ShowTotal);
    }
}
