using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// Actual tagged spending in one month, on one account, in one direction.
/// </summary>
/// <param name="Amount">A positive magnitude. Direction is carried by <paramref name="Direction"/>.</param>
internal readonly record struct TaggedSpend(Guid AccountId, DateOnly Month, int TagId, TransactionType Direction, decimal Amount);

/// <summary>
/// Reads what was actually spent or received against the tags a plan's items carry, so a planned
/// item can be measured against what really happened.
/// </summary>
internal interface IPlannedItemMatcher
{
    Task<IReadOnlyList<TaggedSpend>> GetTaggedSpend(
        IEnumerable<Guid> accountIds,
        IEnumerable<int> tagIds,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
