namespace Asm.MooBank.Institution.Macquarie.Domain;

internal interface ITransactionRawRepository : Asm.Domain.IRepository<TransactionRaw, Guid>
{
    Task<IEnumerable<TransactionRaw>> GetAll(Guid instrumentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionRawSummary>> GetSummaries(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<TransactionRaw> transactions);

    Task<TransactionRaw?> GetZeroBalance(Guid accountId, string details, DateOnly transactionTime, decimal debit, decimal credit, CancellationToken cancellationToken = default);
}
