using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// What a plan's items actually come to once real spending is taken into account.
/// </summary>
/// <param name="IncomeByMonth">Income per month, realised where an item is tagged.</param>
/// <param name="ExpensesByMonth">Planned expenses per month, realised where an item is tagged.</param>
/// <param name="AttributedByMonth">
/// Actual spending claimed by planned items, per month. Subtracted wherever ordinary spending is
/// inferred from the transaction record, because spending a planned item claims is by definition
/// not baseline.
/// </param>
internal sealed record RealisedPlan(
    Dictionary<string, decimal> IncomeByMonth,
    Dictionary<string, decimal> ExpensesByMonth,
    Dictionary<string, decimal> AttributedByMonth,
    IReadOnlyList<PlannedItemProgress> Progress)
{
    public static RealisedPlan Empty => new([], [], [], []);
}

/// <summary>
/// Measures planned items against the spending that actually carried their tag, and rewrites the
/// plan's monthly figures to match.
/// </summary>
/// <remarks>
/// The organising rule: <em>baseline outgoings are the spending not covered by a planned item</em>.
/// A transaction carrying a planned item's tag is that item's spending and is never baseline.
///
/// Matching is bounded rather than eager. There is no search for amounts that look about right and
/// no inference that a payment must have been some item: spending outside an item's claim window is
/// ordinary spending, and the item is reported unmatched. Keeping the plan close to reality is the
/// author's job; this only has to make the divergence visible.
///
/// An item with no tag is left exactly as planned, so realisation is opt-in per item.
/// </remarks>
internal static class PlannedItemRealiser
{
    public static RealisedPlan Realise(
        DomainForecastPlan plan,
        IReadOnlyList<TaggedSpend> spend,
        IReadOnlyCollection<Guid> historicalAccountIds,
        DateOnly latestTransactionMonth,
        int slippageMonths)
    {
        var items = plan.PlannedItems.Where(i => i.IsIncluded).ToList();

        if (items.Count == 0) return RealisedPlan.Empty;

        var allocations = items.ToDictionary(i => i.Id, i => PlannedItemExpander.Allocate(i, plan.StartDate, plan.EndDate));
        var claims = items.ToDictionary(i => i.Id, i => PlannedItemExpander.ClaimWindow(i, plan.EndDate, slippageMonths));

        // Attribution runs twice over the same rows, because two different questions are being
        // asked. What an item has cost counts every payment: a car paid for out of savings is still
        // the car, and a purchase kept out of the reports still emptied the account. What may be
        // taken back out of the baseline counts only what the baseline could have contained —
        // spending on the accounts those figures were computed over, and visible to reporting.
        var attributedAll = Attribute(items, allocations, claims, spend, _ => true);
        var attributedInBaseline = Attribute(items, allocations, claims, spend,
            s => s.InReporting && historicalAccountIds.Contains(s.AccountId));

        var incomeByMonth = new Dictionary<string, decimal>();
        var expensesByMonth = new Dictionary<string, decimal>();
        var progress = new List<PlannedItemProgress>();

        foreach (var item in items)
        {
            var attributed = attributedAll.GetValueOrDefault(item.Id) ?? [];
            var target = item.ItemType == PlannedItemType.Income ? incomeByMonth : expensesByMonth;

            foreach (var (monthKey, amount) in Contributions(item, allocations[item.Id], claims[item.Id], attributed, latestTransactionMonth))
            {
                target[monthKey] = target.GetValueOrDefault(monthKey, 0m) + amount;
            }

            progress.Add(Progress(item, allocations[item.Id], claims[item.Id], attributed, latestTransactionMonth));
        }

        var attributedByMonth = new Dictionary<string, decimal>();
        foreach (var item in items.Where(i => i.ItemType == PlannedItemType.Expense))
        {
            foreach (var (monthKey, amount) in attributedInBaseline.GetValueOrDefault(item.Id) ?? [])
            {
                attributedByMonth[monthKey] = attributedByMonth.GetValueOrDefault(monthKey, 0m) + amount;
            }
        }

        return new RealisedPlan(incomeByMonth, expensesByMonth, attributedByMonth, progress);
    }

    /// <summary>
    /// Shares actual tagged spending out among the items claiming it.
    /// </summary>
    /// <remarks>
    /// Two items can legitimately share a tag — one per child against a single "School Fees" tag,
    /// say — so a month claimed by both is split in proportion to what each planned for it. Where
    /// neither planned anything that month, which is what a payment arriving early or late looks
    /// like, the split falls back to their totals.
    /// </remarks>
    private static Dictionary<Guid, Dictionary<string, decimal>> Attribute(
        List<DomainForecastPlannedItem> items,
        Dictionary<Guid, Dictionary<string, decimal>> allocations,
        Dictionary<Guid, (DateOnly First, DateOnly Last)?> claims,
        IReadOnlyList<TaggedSpend> spend,
        Func<TaggedSpend, bool> counts)
    {
        var result = items.ToDictionary(i => i.Id, _ => new Dictionary<string, decimal>());

        var relevant = spend.Where(counts);

        foreach (var group in relevant.GroupBy(s => (s.TagId, s.Month, s.Direction)))
        {
            var (tagId, month, direction) = group.Key;
            var monthKey = month.ToString("yyyy-MM");
            var total = group.Sum(s => s.Amount);

            if (total == 0m) continue;

            var claimants = items
                .Where(i => i.TagId == tagId)
                .Where(i => DirectionOf(i) == direction)
                .Where(i => claims[i.Id] is { } window && month >= window.First && month <= window.Last)
                .ToList();

            if (claimants.Count == 0) continue;

            var shares = claimants.ToDictionary(i => i.Id, i => allocations[i.Id].GetValueOrDefault(monthKey, 0m));
            var shareTotal = shares.Values.Sum();

            // Nobody planned anything for this month, so fall back to relative size.
            if (shareTotal == 0m)
            {
                shares = claimants.ToDictionary(i => i.Id, i => i.Amount);
                shareTotal = shares.Values.Sum();
            }

            foreach (var claimant in claimants)
            {
                var share = shareTotal == 0m
                    ? total / claimants.Count
                    : total * shares[claimant.Id] / shareTotal;

                result[claimant.Id][monthKey] = result[claimant.Id].GetValueOrDefault(monthKey, 0m) + share;
            }
        }

        return result;
    }

    /// <summary>
    /// What an item actually contributes to each month, once realised.
    /// </summary>
    /// <remarks>
    /// A month that has already happened contributes what was really spent, not what was planned —
    /// which is what makes a bill that came in high, or arrived a month late, or never arrived at
    /// all, stop distorting the figures.
    ///
    /// For a fixed total, whatever has not been spent yet is still owed and is re-spread over the
    /// months the item has left. A one-off has only one such month, so once its date has passed
    /// there is nowhere to put the remainder — and dropping it would quietly make the forecast
    /// optimistic — so while the claim window is still open the remainder moves to the next month
    /// not yet settled. Once the window has closed the shortfall is written off, which is what stops
    /// a $200 bill paid at $195 trailing a $5 phantom for the rest of the plan.
    ///
    /// A recurring charge is never "used up": next month's electricity is not paid off by this
    /// month's.
    /// </remarks>
    private static Dictionary<string, decimal> Contributions(
        DomainForecastPlannedItem item,
        Dictionary<string, decimal> allocations,
        (DateOnly First, DateOnly Last)? claim,
        Dictionary<string, decimal> attributed,
        DateOnly latestTransactionMonth)
    {
        // Untagged items cannot be measured against anything, so they stand as planned.
        if (item.TagId is null || claim is null) return allocations;

        var result = new Dictionary<string, decimal>();
        var settled = new Func<string, bool>(key => String.CompareOrdinal(key, latestTransactionMonth.ToString("yyyy-MM")) <= 0);

        foreach (var (monthKey, amount) in attributed.Where(a => settled(a.Key)))
        {
            result[monthKey] = amount;
        }

        var unsettledAllocations = allocations.Where(a => !settled(a.Key)).ToDictionary();

        if (!PlannedItemExpander.HasFiniteTotal(item))
        {
            foreach (var (monthKey, amount) in unsettledAllocations)
            {
                result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + amount;
            }

            return result;
        }

        var remaining = Math.Max(0m, item.Amount - attributed.Values.Sum());

        if (remaining == 0m) return result;

        var plannedAhead = unsettledAllocations.Values.Sum();

        if (plannedAhead > 0m)
        {
            foreach (var (monthKey, amount) in unsettledAllocations)
            {
                result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + (remaining * amount / plannedAhead);
            }
        }
        else if (claim.Value.Last > latestTransactionMonth)
        {
            // Still owed, with no month of its own left to sit in.
            var nextMonth = latestTransactionMonth.AddMonths(1).ToString("yyyy-MM");
            result[nextMonth] = result.GetValueOrDefault(nextMonth, 0m) + remaining;
        }

        return result;
    }

    private static PlannedItemProgress Progress(
        DomainForecastPlannedItem item,
        Dictionary<string, decimal> allocations,
        (DateOnly First, DateOnly Last)? claim,
        Dictionary<string, decimal> attributed,
        DateOnly latestTransactionMonth)
    {
        var actual = attributed.Values.Sum();
        var plannedTotal = PlannedItemExpander.HasFiniteTotal(item) ? item.Amount : allocations.Values.Sum();

        return new PlannedItemProgress
        {
            PlannedItemId = item.Id,
            Name = item.Name,
            PlannedTotal = plannedTotal,
            ActualToDate = actual,
            Remaining = Math.Max(0m, plannedTotal - actual),
            IsMatched = item.TagId is not null,
            IsClosed = item.TagId is not null && claim is { } window && window.Last <= latestTransactionMonth,
        };
    }

    private static TransactionType DirectionOf(DomainForecastPlannedItem item) =>
        item.ItemType == PlannedItemType.Income ? TransactionType.Credit : TransactionType.Debit;
}
