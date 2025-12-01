namespace Asm.MooBank.Domain.Entities.Transactions;

public interface ITransactionRepository : IWritableRepository<Transaction, Guid>
{
    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<Transaction> transactions);
}
