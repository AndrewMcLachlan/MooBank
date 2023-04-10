using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetTagTrendReport : BaseReportQuery, IQuery<TagTrendReport>
{
    public required int TagId { get; init; }

    public required ReportType ReportType { get; init; }
}
