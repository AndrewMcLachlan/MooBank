using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetUserCashFlow : UserReportQuery, IQuery<UserCashFlowReport>;

internal class GetUserCashFlowHandler(IQueryable<LogicalAccount> accounts, IReportRepository repository, User user) : IQueryHandler<GetUserCashFlow, UserCashFlowReport>
{
    public async ValueTask<UserCashFlowReport> Handle(GetUserCashFlow request, CancellationToken cancellationToken)
    {
        var (start, end) = request.ResolveRange();

        var accountIds = await accounts.AccessibleTo(user).Transactional().Select(a => a.Id).ToListAsync(cancellationToken);

        if (accountIds.Count == 0)
        {
            return new()
            {
                Start = start,
                End = end,
                Income = 0,
                Outgoings = 0,
                IncludedAccountIds = [],
            };
        }

        var totals = await repository.GetCreditDebitTotalsForAccounts(accountIds, start, end, cancellationToken);

        decimal income = 0;
        decimal outgoings = 0;

        foreach (var perAccount in totals.Values)
        {
            income += perAccount.Where(t => t.TransactionType == TransactionFilterType.Credit).Sum(t => t.Total);
            outgoings += perAccount.Where(t => t.TransactionType == TransactionFilterType.Debit).Sum(t => t.Total);
        }

        return new()
        {
            Start = start,
            End = end,
            Income = income,
            Outgoings = outgoings,
            IncludedAccountIds = accountIds,
        };
    }
}
