using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class AccountGroupRepository : RepositoryDeleteBase<AccountGroup, Guid>, IAccountGroupRepository
{
    public AccountGroupRepository(MooBankContext dataContext) : base(dataContext) { }

    protected override IQueryable<AccountGroup> GetById(Guid id) => Entities.Where(ag => ag.Id == id);
}
