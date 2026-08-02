#nullable enable
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Services;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Services;

/// <summary>
/// Unit tests for spreading a planned item's money across the months of a plan.
/// </summary>
/// <remarks>
/// The forecast reports whole months, so these are all really one question: does every month the
/// forecast shows get the money it should? A month that misses out is not a small error — a month
/// with no income drags the expense model down to its fixed component, and that one month then sets
/// the low end of the range the whole plan is read from.
/// </remarks>
[Trait("Category", "Unit")]
public class PlannedItemExpanderTests
{
    private static readonly DateOnly PlanStart = new(2024, 12, 1);
    private static readonly DateOnly PlanEnd = new(2033, 12, 1);

    private static DomainPlannedItem Monthly(decimal amount, DateOnly anchor, DateOnly? until = null) =>
        new(Guid.NewGuid())
        {
            Name = "Salary",
            ItemType = PlannedItemType.Income,
            Amount = amount,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Monthly, AnchorDate = anchor, Interval = 1, EndDate = until },
        };

    private static DomainPlannedItem Yearly(string name, decimal amount, DateOnly anchor, DateOnly? until = null) =>
        new(Guid.NewGuid())
        {
            Name = name,
            ItemType = PlannedItemType.Expense,
            Amount = amount,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Yearly, AnchorDate = anchor, Interval = 1, EndDate = until },
        };

    private static decimal Month(Dictionary<string, decimal> allocations, int year, int month) =>
        allocations.GetValueOrDefault(new DateOnly(year, month, 1).ToString("yyyy-MM"), 0m);

    /// <summary>
    /// Given a monthly item anchored late in the month, and a plan ending early in one
    /// When it is spread across the plan
    /// Then the plan's final month should still get its money
    /// </summary>
    /// <remarks>
    /// The defect this pins down, from the real data. A salary anchored on the 30th drifts to the
    /// 28th after the first February, and the plan ends on the 1st of December — so the last
    /// occurrence on or before the end date fell in November and December was modelled with no
    /// income at all. The forecast still showed December, so the expense model was asked what a
    /// household with no income spends, and answered with its fixed component. That single phantom
    /// month then set the bottom of the expense range for the entire plan.
    /// </remarks>
    [Fact]
    public void Allocate_PlanEndingEarlyInAMonth_StillFillsThatMonth()
    {
        var salary = Monthly(12_960m, new DateOnly(2025, 9, 30));

        var allocations = PlannedItemExpander.Allocate(salary, PlanStart, PlanEnd);

        Assert.Equal(12_960m, Month(allocations, 2033, 12));
        Assert.Equal(12_960m, Month(allocations, 2033, 11));
    }

    /// <summary>
    /// Given a monthly item anchored on the 30th
    /// When it runs past a February
    /// Then it should keep landing on the 30th rather than drifting to the 28th
    /// </summary>
    /// <remarks>
    /// Adding a month repeatedly loses the day: once February clamps the 30th to the 28th, every
    /// later occurrence stays on the 28th. It does not change which month the money lands in, so the
    /// forecast is unaffected — but it is why the final month was missed, and a schedule that
    /// silently stops falling on the day it was set for is wrong on its own terms.
    /// </remarks>
    [Fact]
    public void Allocate_MonthlyAnchoredOnThe30th_KeepsItsDayAfterFebruary()
    {
        var salary = Monthly(1_000m, new DateOnly(2025, 1, 30));

        var occurrences = PlannedItemExpander
            .GenerateScheduleOccurrences(salary, PlanStart, new DateOnly(2025, 12, 31))
            .ToList();

        Assert.Equal(new DateOnly(2025, 2, 28), occurrences[1]); // February has no 30th
        Assert.Equal(new DateOnly(2025, 3, 30), occurrences[2]); // ...and the 30th comes back
        Assert.Equal(new DateOnly(2025, 4, 30), occurrences[3]);
    }

    /// <summary>
    /// Given a yearly item whose first occurrence is early in the plan
    /// When it is spread across the plan
    /// Then that month should carry it
    /// </summary>
    /// <remarks>
    /// School fees: one child's start in February 2025 and the other's not until February 2027, so
    /// only one of them should show in the plan's early years.
    /// </remarks>
    [Fact]
    public void Allocate_YearlyItem_LandsInEveryYearFromItsAnchor()
    {
        var xander = Yearly("Xander School Fees", 21_000m, new DateOnly(2025, 2, 4), new DateOnly(2029, 2, 28));
        var felix = Yearly("Felix School Fees", 22_000m, new DateOnly(2027, 2, 4), new DateOnly(2033, 1, 16));

        var xanders = PlannedItemExpander.Allocate(xander, PlanStart, PlanEnd);
        var felixs = PlannedItemExpander.Allocate(felix, PlanStart, PlanEnd);

        Assert.Equal(21_000m, Month(xanders, 2025, 2));
        Assert.Equal(21_000m, Month(xanders, 2026, 2));
        Assert.Equal(21_000m, Month(xanders, 2029, 2));
        Assert.Equal(0m, Month(xanders, 2030, 2)); // its end date has passed

        Assert.Equal(0m, Month(felixs, 2025, 2)); // not started yet
        Assert.Equal(22_000m, Month(felixs, 2027, 2));
    }

    /// <summary>
    /// Given an item whose own end date falls before the plan's
    /// When it is spread across the plan
    /// Then it should stop on its own date, not run to the end of that month's plan year
    /// </summary>
    /// <remarks>
    /// The whole-month treatment applies to the plan's edges, which are only the limit of what is
    /// shown. An item's own end date is a real end and is left alone.
    /// </remarks>
    [Fact]
    public void Allocate_ItemWithItsOwnEndDate_StopsThere()
    {
        var contract = Monthly(3_000m, new DateOnly(2025, 1, 15), until: new DateOnly(2025, 3, 1));

        var allocations = PlannedItemExpander.Allocate(contract, PlanStart, PlanEnd);

        Assert.Equal(3_000m, Month(allocations, 2025, 1));
        Assert.Equal(3_000m, Month(allocations, 2025, 2));
        // The 15th of March is past the item's own end, so March gets nothing.
        Assert.Equal(0m, Month(allocations, 2025, 3));
    }
}
