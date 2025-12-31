using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetAll() : IQuery<IEnumerable<Account>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user)  : IQueryHandler<GetAll, IEnumerable<Account>>
{
    public async ValueTask<IEnumerable<Account>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var accessibleAccountIds = user.Accounts.Union(user.SharedAccounts);
        var all = await accounts.Include(a => a.Bills).Where(a => accessibleAccountIds.Contains(a.Id)).ToListAsync(cancellationToken);

        return all.ToModel();
    }
}
