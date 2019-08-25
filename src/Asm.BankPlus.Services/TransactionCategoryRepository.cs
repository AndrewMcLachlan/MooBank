using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
{
    public class TransactionCategoryRepository : DataRepository, ITransactionCategoryRepository
    {
        public TransactionCategoryRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<TransactionCategory> CreateTransactionCategory(TransactionCategory category)
        {
            Data.Entities.TransactionCategory transactionCategory = category;
            DataContext.Add(transactionCategory);

            await DataContext.SaveChangesAsync();

            return transactionCategory;
        }

        public async Task<TransactionCategory> UpdateTransactionCategory(TransactionCategory category)
        {
            var entity = await GetTransactionCategoryEntity(category.Id);

            entity.Description = category.Description;
            entity.ParentCategoryId = category.ParentCategoryId;

            await DataContext.SaveChangesAsync();

            return entity;
        }
        public async Task<IEnumerable<TransactionCategory>> GetTransactionCategories()
        {
            return (await DataContext.TransactionCategories.Where(t => !t.Deleted).ToListAsync()).Select(t => (TransactionCategory)t).ToList();
        }

        public async Task<TransactionCategory> GetTransactionCategory(int id)
        {
            return await GetTransactionCategoryEntity(id);
        }

        public async Task DeleteTransactionCategory(int id)
        {
            var category = await GetTransactionCategoryEntity(id);

            category.Deleted = true;

            await DataContext.SaveChangesAsync();
        }

        private async Task<Data.Entities.TransactionCategory> GetTransactionCategoryEntity(int id)
        {
            var category = await DataContext.TransactionCategories.Where(c => c.TransactionCategoryId == id).SingleOrDefaultAsync();

            if (category == null) throw new NotFoundException();

            return category;
        }
    }
}
