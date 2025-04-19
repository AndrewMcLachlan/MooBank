using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetBreakdownReport : TypedReportQuery, IQuery<BreakdownReport>
{
    public int? ParentTagId { get; init; } = null;
}

internal class GetBreakdownReportHandler(IReportRepository reportRepository) : IQueryHandler<GetBreakdownReport, BreakdownReport>
{
    public async ValueTask<BreakdownReport> Handle(GetBreakdownReport request, CancellationToken cancellationToken)
    {
        var tagValues = await reportRepository.GetTransactionTagTotals(request.AccountId, request.Start, request.End, request.ReportType, request.ParentTagId, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Tags = tagValues.ToModel(),
        };
    }
}
