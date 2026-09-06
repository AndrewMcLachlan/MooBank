namespace Asm.MooBank.Domain.Entities.Utility;

public interface IAccountRepository : IWritableRepository<Account, Guid>
{
    /// <summary>
    /// Gets an account with one of its bills loaded in full: periods, usages, service charges and
    /// discounts.
    /// </summary>
    /// <remarks>
    /// Only the named bill is loaded. An account accumulates a bill every month, and pulling every
    /// one of their period graphs back to edit a single bill would grow more expensive every year.
    /// </remarks>
    Task<Account?> GetWithBill(Guid instrumentId, int billId, CancellationToken cancellationToken = default);
}
