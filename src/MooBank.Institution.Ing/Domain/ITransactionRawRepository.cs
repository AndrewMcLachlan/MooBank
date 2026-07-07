namespace Asm.MooBank.Institution.Ing.Domain;

internal interface ITransactionRawRepository : Asm.Domain.IRepository<TransactionRaw, Guid>
{
    Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionRawSummary>> GetSummaries(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<TransactionRaw> transactions);
}
