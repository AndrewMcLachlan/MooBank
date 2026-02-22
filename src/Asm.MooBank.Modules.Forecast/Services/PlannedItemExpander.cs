using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Expands planned items from a forecast plan into monthly monetary allocations.
/// </summary>
internal static class PlannedItemExpander
{
    public static Dictionary<string, decimal> ExpandPlannedItems(DomainForecastPlan plan)
    {
        var result = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var sign = item.ItemType == PlannedItemType.Income ? 1m : -1m;

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate)
                        {
                            result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            result[key] = result.GetValueOrDefault(key, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case PlannedItemDateMode.FlexibleWindow when item.FlexibleWindow != null:
                    {
                        var windowStart = item.FlexibleWindow.StartDate < plan.StartDate ? plan.StartDate : item.FlexibleWindow.StartDate;
                        var windowEnd = item.FlexibleWindow.EndDate > plan.EndDate ? plan.EndDate : item.FlexibleWindow.EndDate;

                        if (item.FlexibleWindow.AllocationMode == AllocationMode.AllAtEnd)
                        {
                            var endKey = new DateOnly(windowEnd.Year, windowEnd.Month, 1).ToString("yyyy-MM");
                            result[endKey] = result.GetValueOrDefault(endKey, 0m) + (item.Amount * sign);
                        }
                        else // EvenlySpread
                        {
                            var months = CountMonths(windowStart, windowEnd);
                            if (months > 0)
                            {
                                var amountPerMonth = item.Amount / months;
                                var current = new DateOnly(windowStart.Year, windowStart.Month, 1);
                                var end = new DateOnly(windowEnd.Year, windowEnd.Month, 1);
                                while (current <= end)
                                {
                                    var key = current.ToString("yyyy-MM");
                                    result[key] = result.GetValueOrDefault(key, 0m) + (amountPerMonth * sign);
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return result;
    }

    internal static IEnumerable<DateOnly> GenerateScheduleOccurrences(DomainForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var occurrences = new List<DateOnly>();
        var schedule = item.Schedule!;
        var current = schedule.AnchorDate;
        var endDate = schedule.EndDate ?? planEnd;
        if (endDate > planEnd) endDate = planEnd;

        while (current <= endDate)
        {
            if (current >= planStart)
            {
                occurrences.Add(current);
            }

            current = schedule.Frequency switch
            {
                ScheduleFrequency.Daily => current.AddDays(schedule.Interval),
                ScheduleFrequency.Weekly => current.AddDays(7 * schedule.Interval),
                ScheduleFrequency.Fortnightly => current.AddDays(14 * schedule.Interval),
                ScheduleFrequency.Monthly => AddMonthsWithDay(current, schedule.Interval, schedule.DayOfMonth),
                ScheduleFrequency.Yearly => current.AddYears(schedule.Interval),
                _ => current.AddMonths(1)
            };
        }

        return occurrences;
    }

    /// <summary>
    /// Returns positive expense amounts for non-baseline planned items whose occurrence
    /// dates are on or before the <paramref name="realizedBefore"/> cutoff. These represent
    /// expenses that have been realized as actual transactions and should be excluded from
    /// historical training data to prevent double-counting.
    /// </summary>
    public static Dictionary<string, decimal> ExpandRealizedNonBaselineExpenses(
        DomainForecastPlan plan, DateOnly realizedBefore)
    {
        var result = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded && i.ItemType == PlannedItemType.Expense))
        {
            if (IsBaselineFrequency(item)) continue;

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate && fixedDate <= realizedBefore)
                        {
                            var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                            result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + item.Amount;
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            if (occurrence <= realizedBefore)
                            {
                                var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                                result[key] = result.GetValueOrDefault(key, 0m) + item.Amount;
                            }
                        }
                        break;
                    }

                case PlannedItemDateMode.FlexibleWindow when item.FlexibleWindow != null:
                    {
                        var windowStart = item.FlexibleWindow.StartDate < plan.StartDate ? plan.StartDate : item.FlexibleWindow.StartDate;
                        var windowEnd = item.FlexibleWindow.EndDate > plan.EndDate ? plan.EndDate : item.FlexibleWindow.EndDate;

                        if (item.FlexibleWindow.AllocationMode == AllocationMode.AllAtEnd)
                        {
                            if (windowEnd <= realizedBefore)
                            {
                                var endKey = new DateOnly(windowEnd.Year, windowEnd.Month, 1).ToString("yyyy-MM");
                                result[endKey] = result.GetValueOrDefault(endKey, 0m) + item.Amount;
                            }
                        }
                        else // EvenlySpread
                        {
                            var months = CountMonths(windowStart, windowEnd);
                            if (months > 0)
                            {
                                var amountPerMonth = item.Amount / months;
                                var current = new DateOnly(windowStart.Year, windowStart.Month, 1);
                                var end = new DateOnly(windowEnd.Year, windowEnd.Month, 1);
                                while (current <= end)
                                {
                                    // A month's allocation is realized if the month has fully passed
                                    var monthEnd = current.AddMonths(1).AddDays(-1);
                                    if (monthEnd <= realizedBefore)
                                    {
                                        var key = current.ToString("yyyy-MM");
                                        result[key] = result.GetValueOrDefault(key, 0m) + amountPerMonth;
                                    }
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether a planned item occurs at a baseline frequency (monthly or more often).
    /// Baseline items are consistent across all months and don't create outlier spikes in
    /// historical data.
    /// </summary>
    internal static bool IsBaselineFrequency(DomainForecastPlannedItem item)
    {
        if (item.DateMode != PlannedItemDateMode.Schedule || item.Schedule == null)
            return false;

        return item.Schedule.Frequency switch
        {
            ScheduleFrequency.Daily => true,
            ScheduleFrequency.Weekly => true,
            ScheduleFrequency.Fortnightly => true,
            ScheduleFrequency.Monthly => item.Schedule.Interval <= 1,
            _ => false,
        };
    }

    internal static int CountMonths(DateOnly start, DateOnly end)
    {
        return ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
    }

    private static DateOnly AddMonthsWithDay(DateOnly date, int months, int? dayOfMonth)
    {
        var newDate = date.AddMonths(months);
        if (dayOfMonth.HasValue)
        {
            var maxDay = DateTime.DaysInMonth(newDate.Year, newDate.Month);
            var day = Math.Min(dayOfMonth.Value, maxDay);
            newDate = new DateOnly(newDate.Year, newDate.Month, day);
        }
        return newDate;
    }
}
