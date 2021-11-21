using System.Linq.Expressions;
using System.Reflection;
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

        public async Task<int> GetTotalTransactions(Guid accountId, string filter, DateTime? start, DateTime? end)
        {
            _security.AssertPermission(accountId);

            Expression<Func<Data.Entities.Transaction, bool>> predicate = (t) => (filter == null || t.Description.Contains(filter)) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end);

            return await DataContext.Transactions.Where(t => t.AccountId == accountId).Where(predicate).CountAsync();
        }

        public async Task<int> GetTotalUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end)
        {
            _security.AssertPermission(accountId);

            Expression<Func<Data.Entities.Transaction, bool>> predicate = (t) => (filter == null || t.Description.Contains(filter)) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end);

            return await DataContext.Transactions.Where(t => t.AccountId == accountId && !t.TransactionTags.Any()).Where(predicate).CountAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId)
        {
            _security.AssertPermission(accountId);

            return (await GetTransactionsQuery(accountId).Where(t => t.AccountId == accountId).ToListAsync()).Select(t => (Transaction)t).OrderByDescending(t => t.TransactionTime);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection)
        {
            _security.AssertPermission(accountId);

            Expression<Func<Data.Entities.Transaction, bool>> predicate = (t) => (filter == null || t.Description.Contains(filter)) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end);

            return await GetTransactionsQuery(accountId).Where(predicate).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToModelListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= start && t.TransactionTime <= (end ?? DateTime.Now)).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToModelListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= DateTime.Now.Subtract(period) && t.TransactionTime <= DateTime.Now).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToModelListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection)
        {
            _security.AssertPermission(accountId);

            Expression<Func<Data.Entities.Transaction, bool>> predicate = (t) => (filter == null || t.Description.Contains(filter)) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end);

            return await GetTransactionsQuery(accountId).Where(predicate).Where(t => !t.TransactionTags.Any()).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToModelListAsync();
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
            return DataContext.Transactions.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId);
        }

        private async Task<Data.Entities.Transaction> GetEntity(Guid id)
        {
            Data.Entities.Transaction entity = await DataContext.Transactions.Include(t => t.TransactionTags).SingleOrDefaultAsync(t => t.TransactionId == id);

            if (entity == null) throw new NotFoundException("Transaction not found");

            _security.AssertPermission(entity.AccountId);

            return entity;
        }
    }

    public static class IQueryableExtensions
    {
        private static readonly PropertyInfo[] _transactionProperties;

        static IQueryableExtensions()
        {
            _transactionProperties = typeof(Transaction).GetProperties();
        }

        public static IQueryable<Data.Entities.Transaction> Page(this IQueryable<Data.Entities.Transaction> query, int pageSize, int pageNumber)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public static async Task<IEnumerable<Transaction>> ToModelListAsync(this IQueryable<Data.Entities.Transaction> query)
        {
            return (await query.ToListAsync()).Select(t => (Transaction)t);
        }

        public static IOrderedQueryable<Data.Entities.Transaction> Sort(this IQueryable<Data.Entities.Transaction> query, string field, SortDirection direction)
        {
            if (!String.IsNullOrWhiteSpace(field))
            {
                PropertyInfo property = _transactionProperties.SingleOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));

                if (property == null) throw new ArgumentException($"Unknown field {field}", nameof(field));


                ParameterExpression param = Expression.Parameter(typeof(Data.Entities.Transaction), String.Empty);
                MemberExpression propertyExp = Expression.Property(param, field);
                LambdaExpression sort = Expression.Lambda(propertyExp, param);
                MethodCallExpression call = Expression.Call(typeof(Queryable), "OrderBy" + (direction == SortDirection.Descending ? "Descending" : String.Empty), new[] { typeof(Data.Entities.Transaction), propertyExp.Type },
                query.Expression,
                Expression.Quote(sort));
                return (IOrderedQueryable<Data.Entities.Transaction>)query.Provider.CreateQuery<Data.Entities.Transaction>(call);
            }

            Expression<Func<Data.Entities.Transaction, object>> sortFunc = t => t.TransactionTime;
            return direction == SortDirection.Ascending ? query.OrderBy(sortFunc) : query.OrderByDescending(sortFunc);

        }
    }
}
