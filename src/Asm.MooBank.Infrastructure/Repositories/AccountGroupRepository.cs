using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class AccountGroupRepository : RepositoryDeleteBase<AccountGroup, Guid>, IAccountGroupRepository
{
    public AccountGroupRepository(MooBankContext bankPlusContext) : base(bankPlusContext) { }

    protected override IQueryable<AccountGroup> GetById(Guid id) => DataSet.Where(ag => ag.Id == id);
}
