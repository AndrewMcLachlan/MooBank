using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class GroupRepository(MooBankContext dataContext) : RepositoryDeleteBase<MooBankContext, Group, Guid>(dataContext), IGroupRepository
{
    public override void Delete(Guid id)
    {
        var group = Entities.Find(id) ?? throw new NotFoundException();
        Entities.Remove(group);
    }

    public async Task<int> GetNextSortOrder(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Projected as nullable so an owner with no groups gives null rather than throwing on Max.
        var last = await Entities.Where(g => g.OwnerId == ownerId)
                                 .Select(g => (int?)g.SortOrder)
                                 .MaxAsync(cancellationToken);

        return last is null ? 0 : last.Value + 1;
    }
}
