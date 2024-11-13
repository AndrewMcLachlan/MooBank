using Asm.Cqrs.Queries;
using Asm.MooBank.Modules.Bills.Models;
using Asm.MooBank.Security;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record Get(Guid AccountId) : IQuery<Account>;

internal class GetHandler(IQueryable<Domain.Entities.Utility.Account> accounts, ISecurity security) : IQueryHandler<Get, Account>
{
    public async ValueTask<Account> Handle(Get query, CancellationToken cancellationToken)
    {
        await security.AssertInstrumentPermissionAsync(query.AccountId, cancellationToken);

        var account = await accounts.Where(a => a.Id == query.AccountId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return account.ToModel();
    }
}
