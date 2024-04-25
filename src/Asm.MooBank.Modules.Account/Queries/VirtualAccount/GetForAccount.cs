using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;
public record GetForAccount(Guid AccountId) : IQuery<IEnumerable<Models.Account.VirtualInstrument>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<GetForAccount, IEnumerable<Models.Account.VirtualInstrument>>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Account.VirtualInstrument>> Handle(GetForAccount request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.VirtualInstruments).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        return account != null ? account.VirtualInstruments.ToModel(currencyConverter) : throw new NotFoundException("Account not found");
    }
}
