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

    /// <summary>
    /// Given groups with an order set
    /// When they are listed
    /// Then they should come back in that order, not the order they were stored in
    /// </summary>
    [Fact]
    public async Task Handle_WithSortOrder_ReturnsGroupsInThatOrder()
    {
        // Arrange -- deliberately stored back to front.
        var userId = _mocks.User.Id;
        var last = TestEntities.CreateGroup(name: "Everyday", ownerId: userId, sortOrder: 2);
        var first = TestEntities.CreateGroup(name: "Savings", ownerId: userId, sortOrder: 0);
        var middle = TestEntities.CreateGroup(name: "Investments", ownerId: userId, sortOrder: 1);

        var handler = new GetAllHandler(TestEntities.CreateGroupQueryable(last, first, middle), _mocks.User);

        // Act
        var result = await handler.Handle(new GetAll(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Savings", "Investments", "Everyday"], result.Select(g => g.Name));
    }

    /// <summary>
    /// Given groups that all share a sort order
    /// When they are listed
    /// Then they should fall back to name order rather than an arbitrary one
    /// </summary>
    /// <remarks>
    /// Every group created before the order existed sits at nought until something reorders them.
    /// Without the tie-break the list is whatever order the database returns, which is stable
    /// enough to look deliberate and arbitrary enough to be wrong.
    /// </remarks>
    [Fact]
    public async Task Handle_AllSharingASortOrder_FallsBackToNameOrder()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var handler = new GetAllHandler(TestEntities.CreateGroupQueryable(
            TestEntities.CreateGroup(name: "Savings", ownerId: userId),
            TestEntities.CreateGroup(name: "Everyday", ownerId: userId),
            TestEntities.CreateGroup(name: "Investments", ownerId: userId)), _mocks.User);

        // Act
        var result = await handler.Handle(new GetAll(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(["Everyday", "Investments", "Savings"], result.Select(g => g.Name));
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
