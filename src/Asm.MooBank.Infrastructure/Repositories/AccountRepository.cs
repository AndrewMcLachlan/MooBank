using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories;

public class AccountRepository : RepositoryDeleteBase<Account, Guid>, IAccountRepository
{
    private readonly IUserDataProvider _userDataProvider;

    public AccountRepository(MooBankContext dataContext, IUserDataProvider userDataProvider) : base(dataContext)
    {
        _userDataProvider = userDataProvider;
    }

    protected override IQueryable<Account> GetById(Guid id) => DataSet.Where(a => a.AccountId == id);
}