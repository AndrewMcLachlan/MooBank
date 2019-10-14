using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface ITransactionTagRepository
    {
        Task<TransactionTag> CreateTransactionTag(TransactionTag tag);

        Task<TransactionTag> CreateTransactionTag(string name);

        Task<TransactionTag> UpdateTransactionTag(int id, string name);

        Task<IEnumerable<TransactionTag>> GetTransactionTags();

        Task<TransactionTag> GetTransactionTag(int id);
        Task DeleteTransactionTag(int id);
    }
}
