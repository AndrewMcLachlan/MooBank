using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

public class AccountRepository : RepositoryDeleteBase<Account, Guid>, IAccountRepository
{
    public AccountRepository(MooBankContext dataContext) : base(dataContext)
    {
    }

    protected override IQueryable<Account> GetById(Guid id) => DataSet.Where(a => a.AccountId == id);
}