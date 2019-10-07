using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Microsoft.EntityFrameworkCore;
using Transaction = Asm.BankPlus.Models.Transaction;

namespace Asm.BankPlus.Services
{
    public class TransactionRepository : DataRepository, ITransactionRepository
    {
        public TransactionRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            Data.Entities.Transaction entity = (Data.Entities.Transaction)transaction;
            DataContext.Add(entity);

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId)
        {
            return (await DataContext.Transactions.Include(t => t.TransactionTagLinks).ThenInclude(t => t.TransactionTag).ToListAsync()).Select(t => (Transaction)t);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber)
        {
            return (await DataContext.Transactions.Where(t => t.TransactionTime >= start && t.TransactionTime <= (end ?? DateTime.Now)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()).Cast<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber)
        {
            return (await DataContext.Transactions.Where(t => t.TransactionTime >= DateTime.Now.Subtract(period) && t.TransactionTime <= DateTime.Now).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()).Cast<Transaction>();
        }

        public async Task<Transaction> AddTransactionTag(Guid id, int tagId)
        {
            Data.Entities.Transaction entity = await DataContext.Transactions.Include(t => t.TransactionTagLinks).ThenInclude(t => t.TransactionTag).SingleOrDefaultAsync(t => t.TransactionId == id);

            if (entity == null) throw new NotFoundException("Transaction not found");

            if (entity.TransactionTags.Any(t => t.TransactionTagId == tagId)) throw new ExistsException("Cannot add tag, it already exists");

            entity.TransactionTags.Add(DataContext.TransactionTags.Single(t => t.TransactionTagId == tagId));

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<Transaction> RemoveTransactionTag(Guid id, int tagId)
        {
            Data.Entities.Transaction entity = await DataContext.Transactions.Include(t => t.TransactionTagLinks).ThenInclude(t => t.TransactionTag).SingleOrDefaultAsync(t => t.TransactionId == id);

            if (entity == null) throw new NotFoundException("Transaction not found");

            var tag = entity.TransactionTags.SingleOrDefault(t => t.TransactionTagId == tagId);

            if (tag == null) throw new NotFoundException("Tag not found");

            entity.TransactionTags.Remove(tag);

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }
    }
}
