using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transaction = Asm.BankPlus.Models.Transaction;

namespace Asm.BankPlus.Repository
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber);

        Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber);

        Task<Transaction> AddTransactionTag(Guid id, int tagId);

        Task<Transaction> RemoveTransactionTag(Guid id, int tagId);

        Task<Transaction> CreateTransaction(Transaction transaction);


    }
}
