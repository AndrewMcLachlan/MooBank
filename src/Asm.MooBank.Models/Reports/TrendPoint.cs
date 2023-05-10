namespace Asm.MooBank.Models.Reports;

public record TrendPoint
{
    public required DateOnly Month { get; set; }

    public required decimal Amount { get; init; }

    public decimal? OffsetAmount { get; init; }
}


public static class TrendPointExtensions
{
    public static decimal Average(this IEnumerable<TrendPoint> trendPoints)
    {
        var start = trendPoints.Min(t => t.Month);
        var end = trendPoints.Max(t => t.Month).ToEndOfMonth();

        decimal months = end.DifferenceInMonths(start);

        return Math.Round(Math.Abs(trendPoints.Sum(t => t.Amount) / months));
    }

    public static decimal? AverageOffset(this IEnumerable<TrendPoint> trendPoints)
    {
        var start = trendPoints.Min(t => t.Month);
        var end = trendPoints.Max(t => t.Month).ToEndOfMonth();

        decimal months = end.DifferenceInMonths(start); //(end.Year - start.Year) * 12 + (end.Month - start.Month);

        decimal? result = trendPoints.Sum(t => t.OffsetAmount) / months;

        return result != null ? Math.Round(Math.Abs(result.Value)) : result;
    }
}