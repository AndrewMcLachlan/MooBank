using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetBreakdownReport : TypedReportQuery, IQuery<BreakdownReport>
{
    public int? TagId { get; init; }
}
