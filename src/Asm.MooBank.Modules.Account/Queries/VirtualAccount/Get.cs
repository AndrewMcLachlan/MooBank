using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;

public record Get(Guid AccountId, Guid Id) : IQuery<Models.Account.VirtualAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Account> accounts, AccountHolder accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Account.VirtualAccount>
{
    private readonly IQueryable<Domain.Entities.Account.Account> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.SingleOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        if (account is Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            var virtualAccount = institutionAccount.VirtualAccounts.SingleOrDefault(va => va.AccountId == request.Id) ?? throw new NotFoundException();

            return (Models.Account.VirtualAccount)virtualAccount;
        }
        else
        {
            throw new InvalidOperationException("Virtual accounts are only available for institution accounts.");
        }

    }
}
