namespace Asm.MooBank.Institution.Macquarie.Domain;

internal interface ITransactionRawRepository : Asm.Domain.IRepository<TransactionRaw, Guid>
{
    Task<IEnumerable<TransactionRaw>> GetUnprocessed(IEnumerable<Guid> transactionIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<TransactionRaw> transactions);
}
