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
            Data.Entities.Transaction entity = transaction;
            DataContext.Add(entity);

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId)
        {
            return (await DataContext.Transactions.ToListAsync()).Cast<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber)
        {
            return (await DataContext.Transactions.Where(t => t.TransactionTime >= start && t.TransactionTime <= (end ?? DateTime.Now)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()).Cast<Transaction>();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber)
        {
            return (await DataContext.Transactions.Where(t => t.TransactionTime >= DateTime.Now.Subtract(period) && t.TransactionTime <= DateTime.Now).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync()).Cast<Transaction>();
        }

        public async Task<Transaction> SetTransactionCategory(Guid id, int categoryId)
        {
            Data.Entities.Transaction entity = await DataContext.Transactions.SingleOrDefaultAsync(t => t.TransactionId == id);

            if (entity == null) throw new NotFoundException();

            entity.TransactionCategoryId = categoryId;

            await DataContext.SaveChangesAsync();

            return entity;
        }
    }
}
