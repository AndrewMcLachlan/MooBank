using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Per-month breakdown of planned items with both net and gross components.
/// </summary>
internal sealed record PlannedItemsBreakdown(
    /// <summary>Net amount per month (negative for expenses, positive for income). Used for balance calculations.</summary>
    Dictionary<string, decimal> Net,
    /// <summary>Gross planned expenses per month (positive values). Used for excluding known expenses from historical data.</summary>
    Dictionary<string, decimal> GrossExpenses,
    /// <summary>Gross planned income per month (positive values). Used for excluding known income from historical data.</summary>
    Dictionary<string, decimal> GrossIncome);

/// <summary>
/// Expands planned items from a forecast plan into monthly monetary allocations.
/// </summary>
internal static class PlannedItemExpander
{
    public static PlannedItemsBreakdown ExpandPlannedItems(DomainForecastPlan plan)
    {
        var net = new Dictionary<string, decimal>();
        var grossExpenses = new Dictionary<string, decimal>();
        var grossIncome = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var sign = item.ItemType == PlannedItemType.Income ? 1m : -1m;
            var isIncome = item.ItemType == PlannedItemType.Income;

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate)
                        {
                            var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                            AddToMonth(net, grossExpenses, grossIncome, monthKey, item.Amount, sign, isIncome);
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            AddToMonth(net, grossExpenses, grossIncome, key, item.Amount, sign, isIncome);
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
                            AddToMonth(net, grossExpenses, grossIncome, endKey, item.Amount, sign, isIncome);
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
                                    AddToMonth(net, grossExpenses, grossIncome, key, amountPerMonth, sign, isIncome);
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return new PlannedItemsBreakdown(net, grossExpenses, grossIncome);
    }

    private static void AddToMonth(Dictionary<string, decimal> net, Dictionary<string, decimal> grossExpenses, Dictionary<string, decimal> grossIncome, string monthKey, decimal amount, decimal sign, bool isIncome)
    {
        net[monthKey] = net.GetValueOrDefault(monthKey, 0m) + (amount * sign);

        if (isIncome)
        {
            grossIncome[monthKey] = grossIncome.GetValueOrDefault(monthKey, 0m) + amount;
        }
        else
        {
            grossExpenses[monthKey] = grossExpenses.GetValueOrDefault(monthKey, 0m) + amount;
        }
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
