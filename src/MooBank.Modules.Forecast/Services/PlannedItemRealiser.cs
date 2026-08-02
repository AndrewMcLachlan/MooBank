using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// What a plan's items actually come to once real spending is taken into account.
/// </summary>
/// <param name="IncomeByMonth">Income per month, straight from the plan's income items.</param>
/// <param name="ExpensesByMonth">Planned expenses per month, realised where payments are linked.</param>
/// <param name="AttributedByMonth">
/// Linked spending the baseline could have contained, per month. Subtracted wherever ordinary
/// spending is read from the record, because spending that belongs to a planned item is by
/// definition not baseline.
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
/// Measures planned expenses against the payments the author has linked to them.
/// </summary>
/// <remarks>
/// The organising rule: <em>baseline outgoings are the spending not covered by a planned item</em>.
/// A payment linked to an item is that item's, and is never baseline.
///
/// Only linked payments count. A tag identifies a category, not a project — one "Home Improvements"
/// tag covers the solar panels, the fence and the renovation — so no rule over tags and dates can
/// say which payment belongs to which, and guessing produced confident wrong answers. The tag now
/// does the one job it can do honestly: narrowing the payments offered when the author links them.
///
/// Income is not measured at all. It is the plan's own statement of what will arrive, and nothing is
/// averaged or fitted from it the way it is from expenses, so measuring it would add a class of
/// error and buy nothing.
/// </remarks>
internal static class PlannedItemRealiser
{
    public static RealisedPlan Realise(
        DomainForecastPlan plan,
        IReadOnlyList<LinkedPayment> payments,
        IReadOnlyCollection<Guid> historicalAccountIds,
        DateOnly latestTransactionMonth)
    {
        var items = plan.PlannedItems.Where(i => i.IsIncluded).ToList();

        if (items.Count == 0) return RealisedPlan.Empty;

        var byTransaction = payments.ToDictionary(p => p.TransactionId);

        var incomeByMonth = new Dictionary<string, decimal>();
        var expensesByMonth = new Dictionary<string, decimal>();
        var attributedByMonth = new Dictionary<string, decimal>();
        var progress = new List<PlannedItemProgress>();

        foreach (var item in items)
        {
            var allocations = PlannedItemExpander.Allocate(item, plan.StartDate, plan.EndDate);

            if (item.ItemType == PlannedItemType.Income)
            {
                Add(incomeByMonth, allocations);
                progress.Add(AsPlanned(item, allocations));
                continue;
            }

            var links = item.Transactions
                .Select(t => byTransaction.TryGetValue(t.TransactionId, out var payment) ? payment : (LinkedPayment?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToList();

            if (links.Count == 0)
            {
                Add(expensesByMonth, allocations);
                progress.Add(AsPlanned(item, allocations));
                continue;
            }

            var actualByMonth = links
                .GroupBy(p => p.Month.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            Add(expensesByMonth, Contributions(item, allocations, actualByMonth, latestTransactionMonth));

            // Only what the baseline could have held comes back out of it: spending on the accounts
            // those figures were computed over, and visible to reporting. A payment kept out of the
            // reports still paid the item, but it was never in the baseline to remove.
            foreach (var payment in links.Where(p => p.InReporting && historicalAccountIds.Contains(p.AccountId)))
            {
                var monthKey = payment.Month.ToString("yyyy-MM");
                attributedByMonth[monthKey] = attributedByMonth.GetValueOrDefault(monthKey, 0m) + payment.Amount;
            }

            progress.Add(Measured(item, allocations, actualByMonth, latestTransactionMonth));
        }

        return new RealisedPlan(incomeByMonth, expensesByMonth, attributedByMonth, progress);
    }

    /// <summary>
    /// What a linked item contributes to each month.
    /// </summary>
    /// <remarks>
    /// A month that has already happened contributes what was actually paid, which is what stops a
    /// bill that came in high, or arrived a month late, or was settled in instalments from
    /// distorting the figures.
    ///
    /// Whatever has not been paid yet is still owed, and is re-spread over the months the item has
    /// left. A one-off has only one such month, so once its date has passed there is nowhere to put
    /// the remainder — and dropping it would quietly make the forecast optimistic by exactly the
    /// amount outstanding — so it moves to the next month not yet settled.
    /// </remarks>
    private static Dictionary<string, decimal> Contributions(
        DomainForecastPlannedItem item,
        Dictionary<string, decimal> allocations,
        Dictionary<string, decimal> actualByMonth,
        DateOnly latestTransactionMonth)
    {
        var settledThrough = latestTransactionMonth.ToString("yyyy-MM");
        var result = new Dictionary<string, decimal>(actualByMonth);

        var unsettled = allocations
            .Where(a => String.CompareOrdinal(a.Key, settledThrough) > 0)
            .ToDictionary();

        if (!PlannedItemExpander.HasFiniteTotal(item))
        {
            // A recurring charge is never used up: paying this month's electricity does nothing
            // about next month's, so each occurrence answers for itself.
            //
            // An occurrence a payment has settled is answered by it. One nothing has been linked to
            // stands as planned, whether or not its month has been and gone: no link is absence of
            // information, not evidence that nothing was spent.
            //
            // A payment settles the occurrence it is nearest to, rather than only the one falling in
            // the very month it was paid. Which occurrence a payment clears is arithmetic on dates,
            // not a guess about intent -- the author has already said the payment is this item's.
            // Matching by month alone meant a bill paid a few days early settled nothing: the 2025
            // school fees, paid on 29 January against a February occurrence, were counted once as
            // the payment and again as the plan.
            var settled = allocations.Keys
                .Where(occurrence => actualByMonth.Keys.Any(paid => NearestOccurrence(paid, allocations.Keys) == occurrence))
                .ToHashSet();

            foreach (var (monthKey, amount) in allocations)
            {
                if (settled.Contains(monthKey)) continue;

                result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + amount;
            }

            return result;
        }

        var remaining = Math.Max(0m, item.Amount - actualByMonth.Values.Sum());

        if (remaining == 0m) return result;

        var plannedAhead = unsettled.Values.Sum();

        if (plannedAhead > 0m)
        {
            foreach (var (monthKey, amount) in unsettled)
            {
                result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + (remaining * amount / plannedAhead);
            }
        }
        else
        {
            var nextMonth = latestTransactionMonth.AddMonths(1).ToString("yyyy-MM");
            result[nextMonth] = result.GetValueOrDefault(nextMonth, 0m) + remaining;
        }

        return result;
    }

    /// <summary>
    /// The occurrence a payment settles: whichever falls closest to the month it was paid in.
    /// </summary>
    private static string? NearestOccurrence(string paidMonth, IEnumerable<string> occurrences)
    {
        var paid = DateOnly.ParseExact(paidMonth + "-01", "yyyy-MM-dd");

        return occurrences
            .OrderBy(o => Math.Abs(MonthsBetween(DateOnly.ParseExact(o + "-01", "yyyy-MM-dd"), paid)))
            .ThenBy(o => o, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int MonthsBetween(DateOnly from, DateOnly to) =>
        ((to.Year - from.Year) * 12) + to.Month - from.Month;

    /// <summary>An item nothing has been measured against, standing exactly as planned.</summary>
    private static PlannedItemProgress AsPlanned(DomainForecastPlannedItem item, Dictionary<string, decimal> allocations)
    {
        var plannedTotal = PlannedItemExpander.HasFiniteTotal(item) ? item.Amount : allocations.Values.Sum();

        return new PlannedItemProgress
        {
            PlannedItemId = item.Id,
            Name = item.Name,
            PlannedTotal = plannedTotal,
            ActualToDate = 0m,
            Remaining = plannedTotal,
            IsMatched = false,
            IsClosed = false,
        };
    }

    private static PlannedItemProgress Measured(
        DomainForecastPlannedItem item,
        Dictionary<string, decimal> allocations,
        Dictionary<string, decimal> actualByMonth,
        DateOnly latestTransactionMonth)
    {
        var actual = actualByMonth.Values.Sum();
        var plannedTotal = PlannedItemExpander.HasFiniteTotal(item) ? item.Amount : allocations.Values.Sum();
        var settledThrough = latestTransactionMonth.ToString("yyyy-MM");

        return new PlannedItemProgress
        {
            PlannedItemId = item.Id,
            Name = item.Name,
            PlannedTotal = plannedTotal,
            ActualToDate = actual,
            Remaining = Math.Max(0m, plannedTotal - actual),
            IsMatched = true,
            // Nothing further is expected once a fixed total has no months left ahead of it.
            IsClosed = PlannedItemExpander.HasFiniteTotal(item) &&
                       !allocations.Any(a => String.CompareOrdinal(a.Key, settledThrough) > 0),
        };
    }

    private static void Add(Dictionary<string, decimal> target, Dictionary<string, decimal> source)
    {
        foreach (var (monthKey, amount) in source)
        {
            target[monthKey] = target.GetValueOrDefault(monthKey, 0m) + amount;
        }
    }
}
