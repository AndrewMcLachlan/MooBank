using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutAverageReport : ReportQuery, IQuery<InOutReport>
{
    public ReportInterval Interval { get; init; } = ReportInterval.Monthly;
}

internal class GetInOutAverageReportHandler(IReportRepository repository) : IQueryHandler<GetInOutAverageReport, InOutReport>
{
    public async ValueTask<InOutReport> Handle(GetInOutAverageReport request, CancellationToken cancellationToken)
    {
        var results = await repository.GetCreditDebitAverages(request.AccountId, request.Start, request.End, request.Interval, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = results.Where(t => t.TransactionType == TransactionFilterType.Credit).Sum(t => t.Average),
            Outgoings = results.Where(t => t.TransactionType == TransactionFilterType.Debit).Sum(t => t.Average),
        };
    }
}
