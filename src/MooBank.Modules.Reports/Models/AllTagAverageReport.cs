namespace Asm.MooBank.Modules.Reports.Models;

public record AllTagAverageReport : TypedReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = [];
}
