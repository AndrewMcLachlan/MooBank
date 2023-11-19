namespace Asm.MooBank.Models.Reports;

public record AllTagAverageReport : TypedReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}
