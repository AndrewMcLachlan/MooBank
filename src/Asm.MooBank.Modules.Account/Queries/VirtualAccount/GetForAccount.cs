using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;
public record GetForAccount(Guid AccountId) : IQuery<IEnumerable<Models.Account.VirtualAccount>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<GetForAccount, IEnumerable<Models.Account.VirtualAccount>>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Account.VirtualAccount>> Handle(GetForAccount request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.VirtualAccounts).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        return account != null ? account.VirtualAccounts.ToModel(currencyConverter) : throw new NotFoundException("Account not found");
    }
}
