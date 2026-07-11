using Asm.MooBank.Domain.Entities.Transactions;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository(MooBankContext dataContext) : RepositoryWriteBase<MooBankContext, Transaction, Guid>(dataContext), ITransactionRepository
{
    // Transaction loads serve write/system paths (rules, imports run without a user context), and a
    // transaction's EXISTING split tags are facts the split reconciliation must see in full — both
    // Tag filters are lifted. This cannot apply a soft-deleted tag: tags are only ever *added* from
    // rule loads, which keep the SoftDelete filter active.

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(instrumentId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).IgnoreQueryFilters().Where(t => t.AccountId == instrumentId && t.InstitutionAccountId == institutionAccountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, IEnumerable<Guid> transactionIds, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(instrumentId).Where(t => transactionIds.Contains(t.Id)).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Guid>> GetTransactionIds(Guid instrumentId, Guid? institutionAccountId = null, CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking()
                      .Where(t => t.AccountId == instrumentId && (institutionAccountId == null || t.InstitutionAccountId == institutionAccountId))
                      .Select(t => t.Id)
                      .ToListAsync(cancellationToken);

    public async Task<IEnumerable<TransactionDescription>> GetTransactionDescriptions(Guid instrumentId, CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking()
                      .Where(t => t.AccountId == instrumentId)
                      .Select(t => new TransactionDescription(t.Id, t.Description))
                      .ToListAsync(cancellationToken);

    private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).IgnoreQueryFilters().Where(t => t.AccountId == accountId);
}
