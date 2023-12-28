using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class AccountGroupRepository(MooBankContext dataContext) : RepositoryDeleteBase<AccountGroup, Guid>(dataContext), IAccountGroupRepository
{
    protected override IQueryable<AccountGroup> GetById(Guid id) => Entities.Where(ag => ag.Id == id);
}
