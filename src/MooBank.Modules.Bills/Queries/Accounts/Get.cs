using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record Get(Guid InstrumentId) : IQuery<Account>;

internal class GetHandler(IQueryable<Domain.Entities.Utility.Account> accounts) : IQueryHandler<Get, Account>
{
    public async ValueTask<Account> Handle(Get query, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.Bills).Where(a => a.Id == query.InstrumentId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return account.ToModel();
    }
}
