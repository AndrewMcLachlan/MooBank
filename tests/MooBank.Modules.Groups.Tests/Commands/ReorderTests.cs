#nullable enable
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Models;
using Asm.MooBank.Modules.Groups.Tests.Support;
using Microsoft.AspNetCore.Http;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;

namespace Asm.MooBank.Modules.Groups.Tests.Commands;

/// <summary>
/// Unit tests for putting a user's groups in a given order.
/// </summary>
/// <remarks>
/// The command takes the whole list, so most of what can go wrong is a list that does not match
/// what the user actually has. A half-applied order is worse than a refused one: the groups left
/// out keep positions that now mean something different, and the list comes back interleaved.
/// </remarks>
[Trait("Category", "Unit")]
public class ReorderTests
{
    private readonly TestMocks _mocks = new();

    private ReorderHandler CreateHandler(IEnumerable<DomainGroup> groups)
    {
        foreach (var group in groups)
        {
            _mocks.GroupRepositoryMock
                .Setup(r => r.Get(group.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);
        }

        return new ReorderHandler(
            _mocks.GroupRepositoryMock.Object,
            TestEntities.CreateGroupQueryable(groups),
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object,
            _mocks.User);
    }

    /// <summary>
    /// Given three groups belonging to the user
    /// When they are reordered
    /// Then each should take the position it was listed in
    /// </summary>
    [Fact]
    public async Task Handle_FullList_AssignsPositionsInOrder()
    {
        // Arrange
        var first = TestEntities.CreateGroup(name: "Savings", ownerId: _mocks.User.Id, sortOrder: 0);
        var second = TestEntities.CreateGroup(name: "Investments", ownerId: _mocks.User.Id, sortOrder: 1);
        var third = TestEntities.CreateGroup(name: "Everyday", ownerId: _mocks.User.Id, sortOrder: 2);

        var handler = CreateHandler([first, second, third]);

        // Act — reversed
        var result = await handler.Handle(
            new Reorder(new GroupOrder { GroupIds = [third.Id, second.Id, first.Id] }),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0, third.SortOrder);
        Assert.Equal(1, second.SortOrder);
        Assert.Equal(2, first.SortOrder);
        Assert.Equal([third.Id, second.Id, first.Id], result.Select(g => g.Id));
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a user with three groups
    /// When the order names only two of them
    /// Then it should be refused, and nothing moved
    /// </summary>
    /// <remarks>
    /// This is what a client working from a stale copy sends. Applying it would leave the missing
    /// group on a position that now collides with a listed one.
    /// </remarks>
    [Fact]
    public async Task Handle_ListMissingAGroup_IsRefusedAndChangesNothing()
    {
        // Arrange
        var first = TestEntities.CreateGroup(ownerId: _mocks.User.Id, sortOrder: 0);
        var second = TestEntities.CreateGroup(ownerId: _mocks.User.Id, sortOrder: 1);
        var third = TestEntities.CreateGroup(ownerId: _mocks.User.Id, sortOrder: 2);

        var handler = CreateHandler([first, second, third]);

        // Act / Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(
            new Reorder(new GroupOrder { GroupIds = [third.Id, first.Id] }),
            TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(0, first.SortOrder);
        Assert.Equal(1, second.SortOrder);
        Assert.Equal(2, third.SortOrder);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given an order naming a group belonging to somebody else
    /// When it is applied
    /// Then it should be refused
    /// </summary>
    /// <remarks>
    /// The stranger's group is not among the caller's, so the sets cannot match however many ids
    /// are sent. Ownership is settled before anything is loaded, not by the assert inside the loop.
    /// </remarks>
    [Fact]
    public async Task Handle_ListNamingSomebodyElsesGroup_IsRefused()
    {
        // Arrange
        var mine = TestEntities.CreateGroup(ownerId: _mocks.User.Id);
        var theirs = TestEntities.CreateGroup(ownerId: Guid.NewGuid());

        var handler = CreateHandler([mine, theirs]);

        // Act / Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(
            new Reorder(new GroupOrder { GroupIds = [theirs.Id, mine.Id] }),
            TestContext.Current.CancellationToken).AsTask());

        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given an order naming the same group twice
    /// When it is applied
    /// Then it should be refused
    /// </summary>
    /// <remarks>
    /// Caught on its own rather than by the count check: a duplicate plus a missing group has the
    /// right length, and would otherwise silently drop the one left out.
    /// </remarks>
    [Fact]
    public async Task Handle_ListWithADuplicate_IsRefused()
    {
        // Arrange
        var first = TestEntities.CreateGroup(ownerId: _mocks.User.Id);
        var second = TestEntities.CreateGroup(ownerId: _mocks.User.Id);

        var handler = CreateHandler([first, second]);

        // Act / Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(
            new Reorder(new GroupOrder { GroupIds = [first.Id, first.Id] }),
            TestContext.Current.CancellationToken).AsTask());

        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
