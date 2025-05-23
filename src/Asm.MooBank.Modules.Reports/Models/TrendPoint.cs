﻿namespace Asm.MooBank.Modules.Reports.Models;

public record TrendPoint
{
    public required DateOnly Month { get; set; }

    public required decimal GrossAmount { get; init; }

    public decimal? NetAmount { get; init; }
}


public static class TrendPointExtensions
{
    public static decimal Average(this IEnumerable<TrendPoint> trendPoints)
    {
        if (!trendPoints.Any()) return 0;

        var start = trendPoints.Min(t => t.Month);
        var end = trendPoints.Max(t => t.Month).ToEndOfMonth();

        // If the difference in months is 0, then we have a single month, so we'll use 1 instead.
        decimal months = Math.Max(end.DifferenceInMonths(start), 1);

        return Math.Round(Math.Abs(trendPoints.Sum(t => t.GrossAmount) / months));
    }

    public static decimal? AverageOffset(this IEnumerable<TrendPoint> trendPoints)
    {
        if (!trendPoints.Any()) return null;

        var start = trendPoints.Min(t => t.Month);
        var end = trendPoints.Max(t => t.Month).ToEndOfMonth();

        // If the difference in months is 0, then we have a single month, so we'll use 1 instead.
        decimal months = Math.Max(end.DifferenceInMonths(start), 1);

        decimal? result = trendPoints.Sum(t => t.NetAmount) / months;

        return result != null ? Math.Round(Math.Abs(result.Value)) : result;
    }

    public static IEnumerable<TrendPoint> ToModel(this IEnumerable<Domain.Entities.Reports.MonthlyTagTotal> trendPoints) =>
        trendPoints.Select(t => new TrendPoint
        {
            Month = t.Month,
            GrossAmount = t.GrossAmount,
            NetAmount = t.NetAmount,
        });
}
