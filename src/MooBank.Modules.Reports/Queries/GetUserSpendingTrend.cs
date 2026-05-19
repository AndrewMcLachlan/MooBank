using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetUserSpendingTrend : UserReportQuery, IQuery<UserSpendingTrendReport>;

internal class GetUserSpendingTrendHandler(IQueryable<LogicalAccount> accounts, IReportRepository repository, User user) : IQueryHandler<GetUserSpendingTrend, UserSpendingTrendReport>
{
    public async ValueTask<UserSpendingTrendReport> Handle(GetUserSpendingTrend request, CancellationToken cancellationToken)
    {
        var (start, end) = request.ResolveRange();

        var accountIds = await accounts.AccessibleTo(user).Transactional().Select(a => a.Id).ToListAsync(cancellationToken);

        if (accountIds.Count == 0)
        {
            return new()
            {
                Start = start,
                End = end,
                Months = [],
                IncludedAccountIds = [],
            };
        }

        var perAccount = await repository.GetMonthlyCreditDebitTotalsForAccounts(accountIds, start, end, cancellationToken);

        var months = perAccount.Values
            .SelectMany(rows => rows)
            .GroupBy(r => r.Month)
            .OrderBy(g => g.Key)
            .Select(g => new CashFlowMonth
            {
                Month = g.Key,
                Income = g.Where(r => r.TransactionType == TransactionFilterType.Credit).Sum(r => r.Total),
                Outgoings = g.Where(r => r.TransactionType == TransactionFilterType.Debit).Sum(r => r.Total),
            })
            .ToList();

        return new()
        {
            Start = start,
            End = end,
            Months = months,
            IncludedAccountIds = accountIds,
        };
    }
}
