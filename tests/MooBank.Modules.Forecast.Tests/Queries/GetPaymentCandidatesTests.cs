#nullable enable
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Queries;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Queries;

/// <summary>
/// Unit tests for which months a planned item's payments are offered from.
/// </summary>
/// <remarks>
/// The tag cannot say which payment belongs to an item, so it is used to narrow what the author is
/// asked to choose between. Narrowing badly is its own failure: too tight and the payment is not on
/// offer at all, too loose and it is buried.
/// </remarks>
[Trait("Category", "Unit")]
public class GetPaymentCandidatesTests
{
    private static DomainForecastPlan Plan() => new(Guid.NewGuid())
    {
        Name = "Test Plan",
        FamilyId = Guid.NewGuid(),
        StartDate = new DateOnly(2024, 12, 1),
        EndDate = new DateOnly(2029, 12, 1),
    };

    private static DomainPlannedItem Yearly(DateOnly anchor, DateOnly? until = null) =>
        new(Guid.NewGuid())
        {
            Name = "School Fees",
            ItemType = PlannedItemType.Expense,
            Amount = 21_000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Yearly, AnchorDate = anchor, Interval = 1, EndDate = until },
        };

    /// <summary>
    /// Given a yearly item
    /// When the months to offer payments from are worked out
    /// Then only the months around each occurrence should be offered
    /// </summary>
    /// <remarks>
    /// Offering the item's whole span instead buries five yearly school fee payments in five years
    /// of everything else the school was ever paid for. Each occurrence gets its own window.
    /// </remarks>
    [Fact]
    public void CandidateMonths_YearlyItem_OffersOnlyTheMonthsAroundEachOccurrence()
    {
        var months = GetPaymentCandidatesHandler.CandidateMonths(Yearly(new DateOnly(2025, 2, 4), new DateOnly(2027, 2, 28)), Plan());

        Assert.NotNull(months);

        // February and the two months either side of it, for each year it falls due.
        Assert.Contains(new DateOnly(2025, 2, 1), months);
        Assert.Contains(new DateOnly(2025, 4, 1), months);
        Assert.Contains(new DateOnly(2026, 2, 1), months);
        Assert.Contains(new DateOnly(2027, 2, 1), months);

        // The middle of the year is nowhere near a payment, in any year it recurs.
        Assert.DoesNotContain(new DateOnly(2025, 7, 1), months);
        Assert.DoesNotContain(new DateOnly(2026, 8, 1), months);

        // And it stops when the item does.
        Assert.DoesNotContain(new DateOnly(2028, 2, 1), months);
    }

    /// <summary>
    /// Given a monthly item
    /// When the months to offer payments from are worked out
    /// Then every month should be offered
    /// </summary>
    /// <remarks>
    /// A monthly charge falls due every month, so nothing is narrowed away — which is right, and
    /// the reason the rule is per-occurrence rather than a blanket span.
    /// </remarks>
    [Fact]
    public void CandidateMonths_MonthlyItem_OffersEveryMonth()
    {
        var monthly = new DomainPlannedItem(Guid.NewGuid())
        {
            Name = "Electricity",
            ItemType = PlannedItemType.Expense,
            Amount = 300m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            Schedule = new PlannedItemSchedule { Frequency = ScheduleFrequency.Monthly, AnchorDate = new DateOnly(2025, 1, 15), Interval = 1 },
        };

        var months = GetPaymentCandidatesHandler.CandidateMonths(monthly, Plan());

        Assert.NotNull(months);
        Assert.Contains(new DateOnly(2025, 7, 1), months);
        Assert.Contains(new DateOnly(2026, 8, 1), months);
    }

    /// <summary>
    /// Given a one-off item
    /// When the months to offer payments from are worked out
    /// Then nothing is narrowed, because its single window already says everything
    /// </summary>
    [Fact]
    public void CandidateMonths_OneOff_NarrowsNothing()
    {
        var solar = new DomainPlannedItem(Guid.NewGuid())
        {
            Name = "Solar",
            ItemType = PlannedItemType.Expense,
            Amount = 17_238.40m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new PlannedItemFixedDate { FixedDate = new DateOnly(2026, 3, 6) },
        };

        Assert.Null(GetPaymentCandidatesHandler.CandidateMonths(solar, Plan()));
    }
}
