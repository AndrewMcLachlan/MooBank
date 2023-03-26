using Asm.MooBank.Domain.Repositories;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Domain.Entities.Transactions;

public interface ITransactionRepository : IWritableRepository<Transaction, Guid>
{
    Task<int> GetTransactionCount(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default);

    Task<int> GetUntaggedTransactionCount(Guid accountId, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<Transaction> transactions);

    Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default);
}
