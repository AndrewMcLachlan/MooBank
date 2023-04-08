namespace Asm.MooBank.Models.Reports;

public record InOutReport : ReportBase
{
    public required decimal Income { get; init; }

    public required decimal Outgoings { get; init; }

}
