namespace Asm.MooBank.Models.Reports;

public abstract record TypedReportBase : ReportBase
{
    public required ReportType ReportType { get; init; }
}
