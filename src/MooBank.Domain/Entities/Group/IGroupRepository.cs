namespace Asm.MooBank.Domain.Entities.Group;

public interface IGroupRepository : IDeletableRepository<Group, Guid>, IWritableRepository<Group, Guid>
{
    /// <summary>
    /// Gets the position a new group for this owner should take: one past the last of them, or
    /// zero when the owner has none yet.
    /// </summary>
    /// <remarks>
    /// A new group belongs at the end of the list — the owner put the existing ones where they
    /// wanted them, and an arrival is not entitled to a place among them. Positions are only
    /// meaningful within one owner's groups, so the owner has to be given.
    /// </remarks>
    Task<int> GetNextSortOrder(Guid ownerId, CancellationToken cancellationToken = default);
}
