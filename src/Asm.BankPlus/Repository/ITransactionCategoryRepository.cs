using System.Collections.Generic;
using System.Threading.Tasks;
using Asm.BankPlus.Models;

namespace Asm.BankPlus.Repository
{
    public interface ITransactionCategoryRepository
    {
        Task<TransactionCategory> CreateTransactionCategory(TransactionCategory category);

        Task<TransactionCategory> UpdateTransactionCategory(TransactionCategory category);

        Task<IEnumerable<TransactionCategory>> GetTransactionCategories();

        Task<TransactionCategory> GetTransactionCategory(int id);
        Task DeleteTransactionCategory(int id);
    }
}
