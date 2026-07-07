using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class GroupRepository(MooBankContext dataContext) : RepositoryDeleteBase<MooBankContext, Group, Guid>(dataContext), IGroupRepository
{
    public override void Delete(Guid id)
    {
        var group = Entities.Find(id) ?? throw new NotFoundException();
        Entities.Remove(group);
    }
}
