namespace Asm.MooBank.Modules.Reports.Models;

public record InOutReport : ReportBase
{
    public required decimal Income { get; init; }

    public required decimal Outgoings { get; init; }

}
