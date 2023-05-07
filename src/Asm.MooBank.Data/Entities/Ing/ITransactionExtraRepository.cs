using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.Ing;

public interface ITransactionExtraRepository : IRepository<TransactionExtra>
{
    void AddRange(IEnumerable<TransactionExtra> transactions);

    Task<IEnumerable<Transaction>> GetUnprocessedTransactions(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionExtra>> GetAll(Guid accountId, CancellationToken cancellationToken = default);
}
