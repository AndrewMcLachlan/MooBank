namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// One payment the author has linked to a planned item.
/// </summary>
/// <param name="Amount">A positive magnitude, net of offsets.</param>
/// <param name="InReporting">
/// Whether the payment is visible to the reporting figures. A transaction marked excluded from
/// reporting still moved the money -- so it still pays off its item -- but it was never part of the
/// baseline average or the expense model's training data, so it must not be taken back out of them.
/// </param>
internal readonly record struct LinkedPayment(Guid TransactionId, Guid AccountId, DateOnly Month, decimal Amount, bool InReporting);

/// <summary>
/// Reads the payments behind a plan's links, so its items can be measured against what really
/// happened.
/// </summary>
internal interface IPlannedItemMatcher
{
    Task<IReadOnlyList<LinkedPayment>> GetPayments(
        IEnumerable<Guid> transactionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Which of the given payments are not on any of the given accounts.
    /// </summary>
    /// <remarks>
    /// Identifiers that match no transaction at all count as out of scope: from the caller's side
    /// there is no difference between a payment that is somebody else's and one that does not
    /// exist, and answering differently would say which.
    /// </remarks>
    Task<IReadOnlyList<Guid>> FindOutOfScope(
        IEnumerable<Guid> transactionIds,
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken = default);
}
