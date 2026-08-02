using Asm.MooBank.Domain.Entities.Transactions;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Sums tagged transaction splits by month, so planned items can be measured against what was
/// really spent.
/// </summary>
/// <remarks>
/// Reads transactions directly rather than through <c>IReportReader</c>: the report procedures roll
/// tag hierarchies up into ancestor totals, which is right for reporting and wrong here — a planned
/// item claims the tag it was given, not everything filed beneath it.
///
/// Summed at split level, not transaction level, so a planned item forming part of a larger
/// purchase is measured at its own share rather than the whole receipt. Offsets are netted through
/// the same database function the reports use, so a refunded purchase nets to nought here too.
/// </remarks>
internal class PlannedItemMatcher(IQueryable<DomainTransaction> transactions) : IPlannedItemMatcher
{
    public async Task<IReadOnlyList<TaggedSpend>> GetTaggedSpend(
        IEnumerable<Guid> accountIds,
        IEnumerable<int> tagIds,
        DateOnly from,
        DateOnly to,
        IEnumerable<Guid> excludingTransactionIds,
        CancellationToken cancellationToken = default)
    {
        var accounts = accountIds.Distinct().ToList();
        var tags = tagIds.Distinct().ToList();
        var linked = excludingTransactionIds.Distinct().ToList();

        if (accounts.Count == 0 || tags.Count == 0 || from > to)
        {
            return [];
        }

        var start = from.ToStartOfDay();
        var end = to.ToEndOfDay();

        var rows = await transactions
            // Transactions excluded from reporting are deliberately still read. Excluding a large
            // one-off from the reports is the same instinct as planning for it, so those are exactly
            // the payments a planned item is most likely to be waiting for.
            .Where(t => accounts.Contains(t.AccountId) &&
                        !linked.Contains(t.Id) &&
                        t.TransactionTime >= start &&
                        t.TransactionTime <= end)
            .SelectMany(t => t.Splits, (t, split) => new { Transaction = t, Split = split })
            .SelectMany(x => x.Split.Tags.Where(tag => tags.Contains(tag.Id)), (x, tag) => new
            {
                x.Transaction.AccountId,
                x.Transaction.TransactionTime,
                x.Transaction.TransactionType,
                x.Transaction.ExcludeFromReporting,
                TagId = tag.Id,
                // Split amounts are stored as positive magnitudes whichever way the money went;
                // TransactionType is what says which.
                Amount = TransactionSplit.TransactionSplitNetAmount(x.Transaction.Id, x.Split.Id, x.Split.Amount),
            })
            .GroupBy(x => new { x.AccountId, x.TransactionTime.Year, x.TransactionTime.Month, x.TagId, x.TransactionType, x.ExcludeFromReporting })
            .Select(g => new
            {
                g.Key.AccountId,
                g.Key.Year,
                g.Key.Month,
                g.Key.TagId,
                g.Key.TransactionType,
                g.Key.ExcludeFromReporting,
                Amount = g.Sum(x => x.Amount),
            })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new TaggedSpend(
            r.AccountId,
            new DateOnly(r.Year, r.Month, 1),
            r.TagId,
            r.TransactionType,
            Math.Abs(r.Amount),
            !r.ExcludeFromReporting))];
    }

    public async Task<IReadOnlyList<LinkedPayment>> GetPayments(
        IEnumerable<Guid> transactionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = transactionIds.Distinct().ToList();

        if (ids.Count == 0) return [];

        var rows = await transactions
            .Where(t => ids.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.AccountId,
                t.TransactionTime,
                t.ExcludeFromReporting,
                // The whole transaction, net of offsets. A link is the author saying this payment is
                // the item, so it is taken at face value rather than by split.
                Amount = DomainTransaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount),
            })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new LinkedPayment(
            r.Id,
            r.AccountId,
            new DateOnly(r.TransactionTime.Year, r.TransactionTime.Month, 1),
            Math.Abs(r.Amount),
            !r.ExcludeFromReporting))];
    }
}
