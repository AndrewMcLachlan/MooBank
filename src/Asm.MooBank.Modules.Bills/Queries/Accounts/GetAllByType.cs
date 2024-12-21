using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Accounts;

public record GetAllByType() : IQuery<IEnumerable<AccountTypeSummary>>;

internal class GetAllByTypeHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetByType, IEnumerable<AccountTypeSummary>>
{
    public async ValueTask<AccountTypeSummary> Handle(GetByType query, CancellationToken cancellationToken)
    {
        var filteredAccounts = await accounts.Where(a => a.Viewers.Any(v => v.UserId == user.Id))
                              .Include(a => a.Bills)
                              .ToListAsync(cancellationToken);

        return filteredAccounts.Select(a =>
            new AccountTypeSummary
            {
                UtilityType = a.UtilityType,
                From = filteredAccounts.SelectMany(a => a.Bills).Min(b => b.IssueDate),
                Accounts = filteredAccounts.Select(a => a.Name),
            });
    }
}

