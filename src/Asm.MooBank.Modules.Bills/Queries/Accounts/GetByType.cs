using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetByType(UtilityType Type) : IQuery<IEnumerable<Account>>;

internal class GetByTypeHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetByType, IEnumerable<Account>>
{
    public async ValueTask<AccountTypeSummary> Handle(GetByType query, CancellationToken cancellationToken)
    {
        var filteredAccounts = await accounts.Where(a => a.UtilityType == query.Type)
                              .Where(a => a.Viewers.Any(v => v.UserId == user.Id))
                              .Include(a => a.Bills)
                              .ToListAsync(cancellationToken);

        return new AccountTypeSummary
        {
            UtilityType = query.Type,
            From = filteredAccounts.SelectMany(a => a.Bills).Min(b => b.IssueDate),
            Accounts = filteredAccounts.Select(a => a.Name),
        };
    }
}

