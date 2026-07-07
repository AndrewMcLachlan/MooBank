using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class GroupRepository(MooBankContext dataContext) : RepositoryDeleteBase<MooBankContext, Group, Guid>(dataContext), IGroupRepository
{
    public override void Delete(Guid id)
    {

    }

}
