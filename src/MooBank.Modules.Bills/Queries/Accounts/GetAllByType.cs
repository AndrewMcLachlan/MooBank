using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetAllByType() : IQuery<IEnumerable<AccountTypeSummary>>;

internal class GetAllByTypeHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetAllByType, IEnumerable<AccountTypeSummary>>
{
    public async ValueTask<IEnumerable<AccountTypeSummary>> Handle(GetAllByType query, CancellationToken cancellationToken)
    {
        var accessibleAccountIds = user.Accounts.Union(user.SharedAccounts);
        var filteredAccounts = await accounts.Where(a => accessibleAccountIds.Contains(a.Id))
                              .Include(a => a.Bills)
                              .GroupBy(a => a.UtilityType)
                              .ToListAsync(cancellationToken);

        return filteredAccounts.Select(a =>
            new AccountTypeSummary
            {
                UtilityType = a.Key,
                From = a.SelectMany(a => a.Bills).Min(b => b.IssueDate),
                Accounts = a.Select(a => a.Name),
            });
    }
}

