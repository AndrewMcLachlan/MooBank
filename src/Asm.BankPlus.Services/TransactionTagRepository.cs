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
    public class TransactionTagRepository : DataRepository, ITransactionTagRepository
    {
        public TransactionTagRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<TransactionTag> CreateTransactionTag(string name)
        {
            return await CreateTransactionTag(new TransactionTag { Name = name });
        }

        public async Task<TransactionTag> CreateTransactionTag(TransactionTag tag)
        {
            Data.Entities.TransactionTag transactionTag = tag;
            DataContext.Add(transactionTag);

            await DataContext.SaveChangesAsync();

            return transactionTag;
        }

        public async Task<TransactionTag> UpdateTransactionTag(TransactionTag tag)
        {
            var entity = await GetTransactionTagEntity(tag.Id);

            entity.Name = tag.Name;

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<IEnumerable<TransactionTag>> GetTransactionTags()
        {
            return (await DataContext.TransactionTags.Where(t => !t.Deleted).ToListAsync()).Select(t => (TransactionTag)t).ToList();
        }

        public async Task<TransactionTag> GetTransactionTag(int id)
        {
            return await GetTransactionTagEntity(id);
        }

        public async Task DeleteTransactionTag(int id)
        {
            var tag = await GetTransactionTagEntity(id);

            tag.Deleted = true;

            await DataContext.SaveChangesAsync();
        }

        private async Task<Data.Entities.TransactionTag> GetTransactionTagEntity(int id)
        {
            var tag = await DataContext.TransactionTags.Where(c => c.TransactionTagId == id).SingleOrDefaultAsync();

            if (tag == null) throw new NotFoundException();

            return tag;
        }
    }
}
