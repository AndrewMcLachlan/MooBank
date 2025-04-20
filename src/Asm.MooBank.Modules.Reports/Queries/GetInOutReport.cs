using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetInOutReport : ReportQuery, IQuery<InOutReport>;

internal class GetInOutReportHandler(IReportRepository repository) : IQueryHandler<GetInOutReport, InOutReport>
{
    public async ValueTask<InOutReport> Handle(GetInOutReport request, CancellationToken cancellationToken)
    {
        var results = await repository.GetCreditDebitTotals(request.AccountId, request.Start, request.End, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Income = results.Where(r => r.TransactionType == TransactionFilterType.Credit).Sum(t => t.Total),
            Outgoings = results.Where(r => r.TransactionType == TransactionFilterType.Debit).Sum(t => t.Total),
        };
    }
}
