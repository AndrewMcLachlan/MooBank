namespace Asm.MooBank.Models.Reports;

public record ByTagReport : ReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}
