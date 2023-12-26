namespace Asm.MooBank.Modules.Reports.Models;

public record InOutTrendReport : ReportBase
{
    public required IEnumerable<TrendPoint> Income { get; init; } = Enumerable.Empty<TrendPoint>();

    public required IEnumerable<TrendPoint> Expenses { get; init; } = Enumerable.Empty<TrendPoint>();
}
