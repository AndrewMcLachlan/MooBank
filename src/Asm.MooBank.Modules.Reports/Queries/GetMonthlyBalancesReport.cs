using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetMonthlyBalancesReport : ReportQuery, IQuery<MonthlyBalancesReport>;

internal class GetMonthlyBalancesReportHandler(IReportRepository repository) : IQueryHandler<GetMonthlyBalancesReport, MonthlyBalancesReport>
{
    public async ValueTask<MonthlyBalancesReport> Handle(GetMonthlyBalancesReport query, CancellationToken cancellationToken)
    {
        var results = await repository.GetMonthlyBalances(query.AccountId, query.Start, query.End, cancellationToken);

        return new()
        {
            AccountId = query.AccountId,
            Start = query.Start,
            End = query.End,
            Balances = results.Select(b => new TrendPoint()
            {
                Month = b.PeriodEnd,
                GrossAmount = b.Balance,
            }).ToList(),
        };
    }
}
