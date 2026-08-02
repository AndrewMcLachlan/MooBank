using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Actual tagged spending in one month, on one account, in one direction.
/// </summary>
/// <param name="Amount">A positive magnitude. Direction is carried by <paramref name="Direction"/>.</param>
/// <param name="InReporting">
/// Whether the spending is visible to the reporting figures. A transaction marked excluded from
/// reporting still moved the money -- so it still pays off a planned item -- but it was never part
/// of the baseline average or the regression's training data, so it must not be taken back out of
/// them.
/// </param>
internal readonly record struct TaggedSpend(Guid AccountId, DateOnly Month, int TagId, TransactionType Direction, decimal Amount, bool InReporting);

/// <summary>
/// One payment the author has linked to a planned item.
/// </summary>
internal readonly record struct LinkedPayment(Guid TransactionId, Guid AccountId, DateOnly Month, decimal Amount, bool InReporting);

/// <summary>
/// Reads what was actually spent or received against a plan's items, so they can be measured
/// against what really happened.
/// </summary>
internal interface IPlannedItemMatcher
{
    /// <summary>
    /// Spending carrying the given tags, for items the author has not linked payments to.
    /// </summary>
    /// <param name="excludingTransactionIds">
    /// Payments already linked to an item. They are that item's, so they must not be offered to
    /// another item that happens to share the tag.
    /// </param>
    Task<IReadOnlyList<TaggedSpend>> GetTaggedSpend(
        IEnumerable<Guid> accountIds,
        IEnumerable<int> tagIds,
        DateOnly from,
        DateOnly to,
        IEnumerable<Guid> excludingTransactionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The payments behind a set of links.
    /// </summary>
    Task<IReadOnlyList<LinkedPayment>> GetPayments(
        IEnumerable<Guid> transactionIds,
        CancellationToken cancellationToken = default);
}
