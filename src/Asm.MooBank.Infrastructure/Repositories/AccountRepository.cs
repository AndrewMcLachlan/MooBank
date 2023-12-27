using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

public class AccountRepository(MooBankContext dataContext) : Asm.Domain.Infrastructure.RepositoryDeleteBase<MooBankContext, Account, Guid>(dataContext), IAccountRepository
{
    public override void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<Account> Get(Guid id, CancellationToken cancellationToken = default) =>
        (Entities.Include(a => a.Rules).ThenInclude(a => a.Tags).Where(a => a.Id == id).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException())!;

    public async Task<InstitutionAccount> GetInstitutionAccount(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await GetById(accountId).Include("VirtualAccounts").SingleOrDefaultAsync() ?? throw new NotFoundException();

        if (account is not InstitutionAccount institutionAccount)
            throw new InvalidOperationException("Cannot update virtual account on non-institution account.");

        return institutionAccount;
    }

    public async Task<VirtualAccount> GetVirtualAccount(Guid accountId, Guid institutionAccountId, CancellationToken cancellationToken)
    {
        var account = await GetInstitutionAccount(institutionAccountId, cancellationToken);
        return account.VirtualAccounts.SingleOrDefault(va => va.Id == accountId) ?? throw new NotFoundException();
    }

    protected IQueryable<Account> GetById(Guid id) => Entities.Where(a => a.Id == id);
}