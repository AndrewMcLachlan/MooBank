using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetByType(UtilityType Type) : IQuery<IEnumerable<Account>>;

internal class GetByTypeHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetByType, IEnumerable<Account>>
{
    public async ValueTask<IEnumerable<Account>> Handle(GetByType query, CancellationToken cancellationToken)
    {
        var filteredAccounts = await accounts.Include(a => a.Bills).Where(a => a.UtilityType == query.Type)
                              .Where(a => user.Accounts.Contains(a.Id))
                              .Include(a => a.Bills)
                              .ToListAsync(cancellationToken);

        return filteredAccounts.Select(a => a.ToModel());
    }
}
