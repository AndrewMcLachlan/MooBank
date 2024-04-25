using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class GroupRepository(MooBankContext dataContext) : RepositoryDeleteBase<Group, Guid>(dataContext), IGroupRepository
{
    protected override IQueryable<Group> GetById(Guid id) => Entities.Where(ag => ag.Id == id);
}
