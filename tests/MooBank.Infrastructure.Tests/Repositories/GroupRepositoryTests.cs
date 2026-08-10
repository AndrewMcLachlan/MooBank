#nullable enable
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="GroupRepository"/>, chiefly the position a new group is given.
/// </summary>
/// <remarks>
/// Positions are only meaningful within one owner's groups — the column is not unique across the
/// table — so the interesting cases are an owner with nothing yet, an owner whose positions have
/// gaps in them, and somebody else's groups being counted by mistake.
/// </remarks>
[Trait("Category", "Unit")]
public class GroupRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;

    public GroupRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Given an owner with no groups
    /// When the next position is asked for
    /// Then it should be the first one
    /// </summary>
    [Fact]
    public async Task GetNextSortOrder_OwnerHasNoGroups_ReturnsZero()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetNextSortOrder(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Given an owner whose groups have gaps in their positions
    /// When the next position is asked for
    /// Then it should be one past the last of them
    /// </summary>
    /// <remarks>
    /// Counting the groups instead of reading the highest position would land on one already taken
    /// as soon as the sequence is not contiguous, and nothing keeps it contiguous.
    /// </remarks>
    [Fact]
    public async Task GetNextSortOrder_OwnerHasGroups_ReturnsOnePastTheLast()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        _context.AddRange(
            TestEntities.CreateGroup(ownerId: ownerId, sortOrder: 0),
            TestEntities.CreateGroup(ownerId: ownerId, sortOrder: 1),
            TestEntities.CreateGroup(ownerId: ownerId, sortOrder: 5));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetNextSortOrder(ownerId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(6, result);
    }

    /// <summary>
    /// Given groups belonging to somebody else
    /// When the next position is asked for
    /// Then they should not be counted
    /// </summary>
    [Fact]
    public async Task GetNextSortOrder_SomebodyElsesGroups_AreIgnored()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        _context.AddRange(
            TestEntities.CreateGroup(ownerId: Guid.NewGuid(), sortOrder: 9),
            TestEntities.CreateGroup(ownerId: ownerId, sortOrder: 0));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetNextSortOrder(ownerId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(1, result);
    }

    private GroupRepository CreateRepository() => new(_context);
}
