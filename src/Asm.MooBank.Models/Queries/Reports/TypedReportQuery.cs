using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public abstract record TypedReportQuery : ReportQuery
{
    public required ReportType ReportType { get; init; }
}
