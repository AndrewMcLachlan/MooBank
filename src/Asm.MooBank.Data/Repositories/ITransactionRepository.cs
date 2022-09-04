using Transaction = Asm.MooBank.Models.Transaction;

namespace Asm.MooBank.Data.Repositories;

public interface ITransactionRepository : IDataRepository
{
    Task<int> GetTotalTransactions(Guid accountId, string filter, DateTime? start, DateTime? end);

    Task<int> GetTotalUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection);

    Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection);

    Task<Transaction> AddTransactionTag(Guid id, int tagId);

    Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags);

    Task<Transaction> RemoveTransactionTag(Guid id, int tagId);

    Task<Transaction> CreateTransaction(Transaction transaction);

    Task<IEnumerable<Transaction>> CreateTransactions(IEnumerable<Transaction> transactions);

    Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection);
}
