using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetGroupMonthlyBalancesReport(Guid GroupId, DateOnly Start, DateOnly End) : IQuery<MonthlyBalancesReport>;

internal class GetGroupMonthlyBalancesReportHandler(IReportRepository repository) : IQueryHandler<GetGroupMonthlyBalancesReport, MonthlyBalancesReport>
{
    public async ValueTask<MonthlyBalancesReport> Handle(GetGroupMonthlyBalancesReport query, CancellationToken cancellationToken)
    {
        var results = await repository.GetGroupMonthlyBalances(query.GroupId, query.Start, query.End, cancellationToken);

        return new()
        {
            AccountId = query.GroupId,
            Start = query.Start,
            End = query.End,
            Balances = [.. results.Select(b => new TrendPoint()
            {
                Month = b.PeriodEnd,
                GrossAmount = b.Balance,
            })],
        };
    }
}
