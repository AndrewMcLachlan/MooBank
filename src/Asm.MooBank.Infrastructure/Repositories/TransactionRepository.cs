using Asm.MooBank.Domain.Entities.Transactions;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository(MooBankContext dataContext) : RepositoryWriteBase<MooBankContext, Transaction, Guid>(dataContext), ITransactionRepository
{
    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(instrumentId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == instrumentId && t.InstitutionAccountId == institutionAccountId).ToListAsync(cancellationToken);

    private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == accountId);
}
