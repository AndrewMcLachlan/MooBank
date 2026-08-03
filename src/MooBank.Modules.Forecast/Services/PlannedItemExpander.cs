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

            foreach (var (monthKey, amount) in Allocate(item, plan.StartDate, plan.EndDate))
            {
                result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + amount;
            }
        }

        return (income, expenses);
    }

    /// <summary>
    /// Spreads one item's money across the months of the plan, keyed <c>yyyy-MM</c>.
    /// </summary>
    /// <remarks>
    /// Kept per-item rather than pre-aggregated because realisation has to measure each item against
    /// its own actual spending: once several items are summed into a month there is no way back to
    /// which of them a payment belongs to.
    /// </remarks>
    public static Dictionary<string, decimal> Allocate(DomainForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var result = new Dictionary<string, decimal>();

        // The forecast reports whole months, so allocation runs to the whole months the plan covers
        // rather than to its exact dates. Otherwise the first and last months are systematically
        // under-filled: a plan ending on the 1st of December still shows December, but a monthly
        // schedule falling on the 28th has no occurrence on or before the 1st, so the month is
        // modelled with no income at all -- and a month with no income drags the whole projection
        // down to the fixed part of the expense model.
        planStart = new DateOnly(planStart.Year, planStart.Month, 1);
        planEnd = new DateOnly(planEnd.Year, planEnd.Month, 1).AddMonths(1).AddDays(-1);

        switch (item.DateMode)
        {
            case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                {
                    var fixedDate = item.FixedDate.FixedDate;
                    if (fixedDate >= planStart && fixedDate <= planEnd)
                    {
                        var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                        result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + item.Amount;
                    }
                    break;
                }

            case PlannedItemDateMode.Schedule when item.Schedule != null:
                {
                    foreach (var occurrence in GenerateScheduleOccurrences(item, planStart, planEnd))
                    {
                        var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                        result[key] = result.GetValueOrDefault(key, 0m) + item.Amount;
                    }
                    break;
                }
        }

        return result;
    }

    /// <summary>
    /// Whether an item's money is a fixed total that can be used up, as opposed to a recurring
    /// charge that cannot.
    /// </summary>
    public static bool HasFiniteTotal(DomainForecastPlannedItem item) =>
        item.DateMode is PlannedItemDateMode.FixedDate;

    internal static IEnumerable<DateOnly> GenerateScheduleOccurrences(DomainForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var occurrences = new List<DateOnly>();
        var schedule = item.Schedule!;
        var current = schedule.AnchorDate;

        // An item's own end date is a real end; the plan's is only the edge of what is shown.
        var endDate = schedule.EndDate ?? planEnd;
        if (endDate > planEnd) endDate = planEnd;

        // Guard against a non-positive interval, which would never advance the schedule
        // and loop forever. Commands validate this, but clamp defensively for legacy data.
        var interval = Math.Max(1, schedule.Interval);

        // Monthly schedules keep the day they were anchored on. Adding a month repeatedly does not:
        // once a short month clamps the 30th to the 28th, every later occurrence stays on the 28th,
        // and the schedule silently drifts away from the day it was set for.
        var dayOfMonth = schedule.DayOfMonth ?? schedule.AnchorDate.Day;

        var elapsed = 0;

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
                ScheduleFrequency.Monthly => AddMonthsWithDay(schedule.AnchorDate, interval * (++elapsed), dayOfMonth),
                ScheduleFrequency.Yearly => current.AddYears(interval),
                _ => current.AddMonths(1)
            };
        }

        return occurrences;
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
