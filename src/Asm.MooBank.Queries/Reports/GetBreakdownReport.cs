using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public record GetBreakdownReport : TypedReportQuery, IQuery<BreakdownReport>
{
    public int? TagId { get; init; }
}
