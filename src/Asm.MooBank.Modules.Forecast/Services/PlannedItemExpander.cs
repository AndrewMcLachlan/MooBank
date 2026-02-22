using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Per-month breakdown of planned items with net totals and non-baseline expense spikes.
/// </summary>
/// <param name="Net">Net amount per month (negative for expenses, positive for income). Used for balance calculations.</param>
/// <param name="NonBaselineExpenses">
/// Gross non-baseline planned expenses per month (positive values).
/// These are expenses that don't occur every month (annual, quarterly, one-off) and
/// would create outliers in historical data when realized. Used to clean training data
/// for regression and baseline calculations.
/// </param>
internal sealed record PlannedItemsBreakdown(
    Dictionary<string, decimal> Net,
    Dictionary<string, decimal> NonBaselineExpenses);

/// <summary>
/// Expands planned items from a forecast plan into monthly monetary allocations.
/// </summary>
internal static class PlannedItemExpander
{
    public static PlannedItemsBreakdown ExpandPlannedItems(DomainForecastPlan plan)
    {
        var net = new Dictionary<string, decimal>();
        var nonBaselineExpenses = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var sign = item.ItemType == PlannedItemType.Income ? 1m : -1m;
            var isNonBaselineExpense = item.ItemType == PlannedItemType.Expense && !IsBaselineFrequency(item);

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate)
                        {
                            var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                            net[monthKey] = net.GetValueOrDefault(monthKey, 0m) + (item.Amount * sign);
                            if (isNonBaselineExpense)
                            {
                                nonBaselineExpenses[monthKey] = nonBaselineExpenses.GetValueOrDefault(monthKey, 0m) + item.Amount;
                            }
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            net[key] = net.GetValueOrDefault(key, 0m) + (item.Amount * sign);
                            if (isNonBaselineExpense)
                            {
                                nonBaselineExpenses[key] = nonBaselineExpenses.GetValueOrDefault(key, 0m) + item.Amount;
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
                            var endKey = new DateOnly(windowEnd.Year, windowEnd.Month, 1).ToString("yyyy-MM");
                            net[endKey] = net.GetValueOrDefault(endKey, 0m) + (item.Amount * sign);
                            if (isNonBaselineExpense)
                            {
                                nonBaselineExpenses[endKey] = nonBaselineExpenses.GetValueOrDefault(endKey, 0m) + item.Amount;
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
                                    var key = current.ToString("yyyy-MM");
                                    net[key] = net.GetValueOrDefault(key, 0m) + (amountPerMonth * sign);
                                    if (isNonBaselineExpense)
                                    {
                                        nonBaselineExpenses[key] = nonBaselineExpenses.GetValueOrDefault(key, 0m) + amountPerMonth;
                                    }
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return new PlannedItemsBreakdown(net, nonBaselineExpenses);
    }

    /// <summary>
    /// Determines whether a planned item occurs frequently enough to be part of the
    /// baseline spending pattern. Items that occur every month (or more often) are baseline;
    /// less frequent items (quarterly, annual, one-off) create outlier spikes.
    /// </summary>
    private static bool IsBaselineFrequency(DomainForecastPlannedItem item) =>
        item.DateMode == PlannedItemDateMode.Schedule &&
        item.Schedule is { } schedule &&
        schedule.Frequency switch
        {
            ScheduleFrequency.Daily => true,
            ScheduleFrequency.Weekly => true,
            ScheduleFrequency.Fortnightly => true,
            ScheduleFrequency.Monthly => schedule.Interval <= 1,
            _ => false,
        };

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
