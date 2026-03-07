namespace Asm.MooBank.Modules.Reports.Models;

public record ByTagReport : ReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}
