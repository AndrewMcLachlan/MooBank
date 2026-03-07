namespace Asm.MooBank.Modules.Reports.Models;

public record TagTrendReport : ReportBase
{
    public required int TagId { get; init; }

    public required string TagName { get; init; }

    public required IEnumerable<TrendPoint> Months { get; init; }
    public decimal Average { get; set; }
    public decimal? OffsetAverage { get; set; }
}
