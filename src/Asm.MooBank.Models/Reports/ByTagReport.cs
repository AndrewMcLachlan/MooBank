namespace Asm.MooBank.Models.Reports;

public record ByTagReport : ReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}

public record TagValue
{
    public int? TagId { get; init; }

    public string? TagName { get; init; }

    public decimal Percent { get; set; }

    public decimal Amount { get; init; }
}