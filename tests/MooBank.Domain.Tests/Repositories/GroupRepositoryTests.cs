using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.User;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="GroupRepository"/> class.
/// Tests verify group CRUD operations against an in-memory database.
/// </summary>
public class GroupRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public GroupRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given groups exist
    /// When Get is called
    /// Then all groups should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingGroups_ReturnsAll()
    {
        // Arrange
        var owner = CreateOwner();
        _context.Users.Add(owner);

        var group1 = CreateGroup(Guid.NewGuid(), "Group 1", owner.Id);
        var group2 = CreateGroup(Guid.NewGuid(), "Group 2", owner.Id);

        _context.Groups.AddRange(group1, group2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new GroupRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given a group exists
    /// When Get by id is called
    /// Then the group should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingGroup_ReturnsGroup()
    {
        // Arrange
        var owner = CreateOwner();
        _context.Users.Add(owner);

        var groupId = Guid.NewGuid();
        var group = CreateGroup(groupId, "Test Group", owner.Id);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new GroupRepository(_context);

        // Act
        var result = await repository.Get(groupId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Group", result.Name);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new group
    /// When Add is called
    /// Then the group should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewGroup_PersistsGroup()
    {
        // Arrange
        var owner = CreateOwner();
        _context.Users.Add(owner);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new GroupRepository(_context);
        var group = CreateGroup(Guid.NewGuid(), "New Group", owner.Id);

        // Act
        repository.Add(group);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "New Group", TestContext.Current.CancellationToken);
        Assert.NotNull(savedGroup);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing group
    /// When Update is called with modified name
    /// Then the group should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingGroup_UpdatesGroup()
    {
        // Arrange
        var owner = CreateOwner();
        _context.Users.Add(owner);

        var groupId = Guid.NewGuid();
        var group = CreateGroup(groupId, "Original Name", owner.Id);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new GroupRepository(_context);

        // Act
        group.Name = "Updated Name";
        repository.Update(group);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedGroup = await _context.Groups.FindAsync([groupId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedGroup!.Name);
    }

    #endregion

    private static Group CreateGroup(Guid id, string name, Guid ownerId) =>
        new(id)
        {
            Name = name,
            OwnerId = ownerId,
        };

    private static User CreateOwner() =>
        new(Guid.NewGuid())
        {
            EmailAddress = "owner@test.com",
            Currency = "AUD",
            FamilyId = Guid.NewGuid(),
        };
}
