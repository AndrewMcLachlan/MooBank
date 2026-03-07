namespace Asm.MooBank.Modules.Reports.Models;

public abstract record TypedReportBase : ReportBase
{
    public required ReportType ReportType { get; init; }
}
