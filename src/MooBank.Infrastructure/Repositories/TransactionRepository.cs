using Asm.MooBank.Domain.Entities.Transactions;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository(MooBankContext dataContext) : RepositoryWriteBase<MooBankContext, Transaction, Guid>(dataContext), ITransactionRepository
{
    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(instrumentId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == instrumentId && t.InstitutionAccountId == institutionAccountId).ToListAsync(cancellationToken);

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
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == accountId);
}
