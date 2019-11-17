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
        private readonly ISecurityRepository _security;

        public TransactionRepository(BankPlusContext dataContext, ISecurityRepository security) : base(dataContext)
        {
            _security = security;
        }

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            _security.AssertPermission(transaction.AccountId);

            Data.Entities.Transaction entity = (Data.Entities.Transaction)transaction;
            DataContext.Add(entity);

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<int> GetTotalTransactions(Guid accountId)
        {
            _security.AssertPermission(accountId);

            return await DataContext.Transactions.Where(t => t.AccountId == accountId).CountAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId)
        {
            _security.AssertPermission(accountId);

            return (await GetTransactionsQuery(accountId).Where(t => t.AccountId == accountId).ToListAsync()).Select(t => (Transaction)t).OrderByDescending(t => t.TransactionTime);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, int pageSize, int pageNumber)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Paging(pageSize, pageNumber);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= start && t.TransactionTime <= (end ?? DateTime.Now)).Paging(pageSize, pageNumber);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= DateTime.Now.Subtract(period) && t.TransactionTime <= DateTime.Now).Paging(pageSize, pageNumber);
        }

        public async Task<Transaction> AddTransactionTag(Guid id, int tagId)
        {
            var entity = await GetEntity(id);

            if (entity.TransactionTags.Any(t => t.TransactionTagId == tagId)) throw new ExistsException("Cannot add tag, it already exists");

            entity.TransactionTags.Add(DataContext.TransactionTags.Single(t => t.TransactionTagId == tagId));

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<Transaction> AddTransactionTags(Guid id, IEnumerable<int> tags)
        {
            var entity = await GetEntity(id);

            var existingIds = entity.TransactionTags.Select(t => t.TransactionTagId);

            var filteredTags = tags.Where(t => !existingIds.Contains(t));

            entity.TransactionTags.AddRange(DataContext.TransactionTags.Where(t => filteredTags.Contains(t.TransactionTagId)));

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<Transaction> RemoveTransactionTag(Guid id, int tagId)
        {
            var entity = await GetEntity(id);

            var tag = entity.TransactionTags.SingleOrDefault(t => t.TransactionTagId == tagId);

            if (tag == null) throw new NotFoundException("Tag not found");

            entity.TransactionTags.Remove(tag);

            await DataContext.SaveChangesAsync();

            return (Transaction)entity;
        }

        public async Task<IEnumerable<Transaction>> CreateTransactions(IEnumerable<Transaction> transactions)
        {
            transactions.Select(t => t.AccountId).Distinct().ToList().ForEach(a => _security.AssertPermission(a));

            var entities = transactions.Select(t => (Data.Entities.Transaction)t).ToList();

            DataContext.AddRange(entities);

            await DataContext.SaveChangesAsync();

            return entities.Select(t => (Transaction)t);
        }

        private IQueryable<Data.Entities.Transaction> GetTransactionsQuery(Guid accountId)
        {
            return DataContext.Transactions.Include(t => t.TransactionTagLinks).ThenInclude(t => t.TransactionTag).Where(t => t.AccountId == accountId);
        }

        private async Task<Data.Entities.Transaction> GetEntity(Guid id)
        {
            Data.Entities.Transaction entity = await DataContext.Transactions.Include(t => t.TransactionTagLinks).ThenInclude(t => t.TransactionTag).SingleOrDefaultAsync(t => t.TransactionId == id);

            if (entity == null) throw new NotFoundException("Transaction not found");

            _security.AssertPermission(entity.AccountId);

            return entity;
        }
    }

    public static class IQueryableExtensions
    {
        public static async Task<IEnumerable<Transaction>> Paging(this IQueryable<Data.Entities.Transaction> query, int pageSize, int pageNumber)
        {
            return (await query.Skip((pageNumber - 1) * pageSize).OrderByDescending(t => t.TransactionTime).Take(pageSize).ToListAsync()).Select(t => (Transaction)t);
        }
    }
}
