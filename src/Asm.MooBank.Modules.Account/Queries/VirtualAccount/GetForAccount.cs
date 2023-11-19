using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;
public record GetForAccount(Guid AccountId) : IQuery<IEnumerable<Models.Account.VirtualAccount>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, AccountHolder accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<GetForAccount, IEnumerable<Models.Account.VirtualAccount>>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Account.VirtualAccount>> Handle(GetForAccount request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.VirtualAccounts).SingleOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        return account != null ? account.VirtualAccounts.Select(va => (Models.Account.VirtualAccount)va) : throw new NotFoundException("Account not found");
    }
}
