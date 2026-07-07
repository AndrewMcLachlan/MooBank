namespace Asm.MooBank.Domain.Entities.Transactions;

public interface ITransactionRepository : IWritableRepository<Transaction, Guid>
{
    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, Guid institutionAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the transactions with the given IDs for an instrument, tracked and with splits and tags loaded.
    /// </summary>
    Task<IEnumerable<Transaction>> GetTransactions(Guid instrumentId, IEnumerable<Guid> transactionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the IDs of the transactions for an instrument without loading or tracking the entities.
    /// </summary>
    Task<IEnumerable<Guid>> GetTransactionIds(Guid instrumentId, Guid? institutionAccountId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ID and description of each transaction for an instrument without loading or tracking the entities.
    /// </summary>
    Task<IEnumerable<TransactionDescription>> GetTransactionDescriptions(Guid instrumentId, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<Transaction> transactions);
}
