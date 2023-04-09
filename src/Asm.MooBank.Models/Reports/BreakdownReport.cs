namespace Asm.MooBank.Models.Reports;

public record BreakdownReport : ReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}

public record TagValue
{
    public int? TagId { get; init; }

    public string? TagName { get; init; }

    public required decimal Amount { get; init; }

    public bool HasChildren { get; init; }
}