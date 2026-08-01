using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Expands planned items from a forecast plan into monthly monetary allocations.
/// </summary>
/// <remarks>
/// Income and expenses are never netted against each other. Income is the plan's whole income
/// model and expenses sit on top of the baseline, so a single signed total would hide both.
/// </remarks>
internal static class PlannedItemExpander
{
    /// <summary>
    /// Expands planned items into monthly allocations split by item type. Both dictionaries hold
    /// positive amounts, so income and expense allocations can be charted (and totalled)
    /// independently rather than netted against each other.
    /// </summary>
    public static (Dictionary<string, decimal> Income, Dictionary<string, decimal> Expenses) ExpandPlannedItemsByType(DomainForecastPlan plan)
    {
        var income = new Dictionary<string, decimal>();
        var expenses = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var result = item.ItemType == PlannedItemType.Income ? income : expenses;

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate)
                        {
                            result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + item.Amount;
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            result[key] = result.GetValueOrDefault(key, 0m) + item.Amount;
                        }
                        break;
                    }

                case PlannedItemDateMode.FlexibleWindow when item.FlexibleWindow != null:
                    {
                        var windowStart = item.FlexibleWindow.StartDate < plan.StartDate ? plan.StartDate : item.FlexibleWindow.StartDate;
                        var windowEnd = item.FlexibleWindow.EndDate > plan.EndDate ? plan.EndDate : item.FlexibleWindow.EndDate;

                        if (item.FlexibleWindow.AllocationMode == AllocationMode.AllAtEnd)
                        {
                            // Skip windows that fall entirely outside the plan.
                            if (windowStart <= windowEnd)
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
                                    var key = current.ToString("yyyy-MM");
                                    result[key] = result.GetValueOrDefault(key, 0m) + amountPerMonth;
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return (income, expenses);
    }

    internal static IEnumerable<DateOnly> GenerateScheduleOccurrences(DomainForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var occurrences = new List<DateOnly>();
        var schedule = item.Schedule!;
        var current = schedule.AnchorDate;
        var endDate = schedule.EndDate ?? planEnd;
        if (endDate > planEnd) endDate = planEnd;

        // Guard against a non-positive interval, which would never advance the schedule
        // and loop forever. Commands validate this, but clamp defensively for legacy data.
        var interval = Math.Max(1, schedule.Interval);

        while (current <= endDate)
        {
            if (current >= planStart)
            {
                occurrences.Add(current);
            }

            current = schedule.Frequency switch
            {
                ScheduleFrequency.Daily => current.AddDays(interval),
                ScheduleFrequency.Weekly => current.AddDays(7 * interval),
                ScheduleFrequency.Fortnightly => current.AddDays(14 * interval),
                ScheduleFrequency.Monthly => AddMonthsWithDay(current, interval, schedule.DayOfMonth),
                ScheduleFrequency.Yearly => current.AddYears(interval),
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
