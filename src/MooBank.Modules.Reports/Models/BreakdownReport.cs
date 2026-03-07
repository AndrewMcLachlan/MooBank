namespace Asm.MooBank.Modules.Reports.Models;

public record BreakdownReport : ReportBase
{
    public required IEnumerable<TagValue> Tags { get; init; } = Enumerable.Empty<TagValue>();
}

public record TagValue
{
    public int? TagId { get; init; }

    public string? TagName { get; init; }

    public decimal GrossAmount { get; init; }

    public decimal? NetAmount { get; init; }

    public bool HasChildren { get; init; }
}

public static class TagValueExtensions
{
    public static IEnumerable<TagValue> ToModel(this IEnumerable<Domain.Entities.Reports.TransactionTagTotal> tagValues)
    {
        return tagValues.Select(tv => new TagValue
        {
            TagId = tv.TagId,
            TagName = tv.TagName,
            GrossAmount = tv.GrossAmount,
            NetAmount = tv.NetAmount,
            HasChildren = tv.HasChildren,
        });
    }

    public static IEnumerable<TagValue> ToModel(this IEnumerable<Domain.Entities.Reports.TagAverage> tagValues)
    {
        return tagValues.Select(tv => new TagValue
        {
            TagId = tv.TagId,
            TagName = tv.Name,
            GrossAmount = 0,
            NetAmount = tv.Average,
            HasChildren = false,
        });
    }
}
