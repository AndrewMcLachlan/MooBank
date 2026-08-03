using Asm.MooBank.Domain.Entities.Transactions;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Reads the payments an author has linked to planned items.
/// </summary>
/// <remarks>
/// Offsets are netted through the same database function the reports use, so a payment that was
/// refunded nets to nought here too.
/// </remarks>
internal class PlannedItemMatcher(IQueryable<DomainTransaction> transactions) : IPlannedItemMatcher
{
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
