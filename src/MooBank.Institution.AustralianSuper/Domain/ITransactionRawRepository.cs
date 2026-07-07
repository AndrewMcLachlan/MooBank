using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Institution.AustralianSuper.Domain;

internal interface ITransactionRawRepository : Asm.Domain.IRepository<TransactionRaw, Guid>
{
    Task<IEnumerable<TransactionRaw>> GetAll(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TransactionRawSummary>> GetSummaries(Guid accountId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<TransactionRaw> transactions);
}
