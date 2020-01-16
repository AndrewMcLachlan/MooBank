using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;
using Transaction = Asm.BankPlus.Models.Transaction;

namespace Asm.BankPlus.Repository
{
    public interface ITransactionRepository : IDataRepository
    {
        Task<int> GetTotalTransactions(Guid accountId);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, int pageSize, int pageNumber);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber);

        Task<Transaction> AddTransactionTag(Guid id, int tagId);

        Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags);

        Task<Transaction> RemoveTransactionTag(Guid id, int tagId);

        Task<Transaction> CreateTransaction(Transaction transaction);

        Task<IEnumerable<Transaction>> CreateTransactions(IEnumerable<Transaction> transactions);
    }
}
