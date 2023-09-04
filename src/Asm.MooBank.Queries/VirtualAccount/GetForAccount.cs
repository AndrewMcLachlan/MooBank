using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.VirtualAccount;
public record GetForAccount(Guid AccountId) : IQuery<IEnumerable<Models.VirtualAccount>>;

internal class GetForAccountHandler : QueryHandlerBase, IQueryHandler<GetForAccount, IEnumerable<Models.VirtualAccount>>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts;
    private readonly ISecurity _security;

    public GetForAccountHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, AccountHolder accountHolder, ISecurity security) : base(accountHolder)
    {
        _accounts = accounts;
        _security = security;
    }

    public async Task<IEnumerable<Models.VirtualAccount>> Handle(GetForAccount request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.VirtualAccounts).SingleOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken);

        return account != null ? account.VirtualAccounts.Select(va => ((Models.VirtualAccount)va)) : throw new NotFoundException("Account not found");
    }
}
