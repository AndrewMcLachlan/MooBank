using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Repositories;
using Transaction = Asm.MooBank.Domain.Entities.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        private readonly ISecurityRepository _security;

        public TransactionRepository(BankPlusContext dataContext, ISecurityRepository security) : base(dataContext)
        {
            _security = security;
        }

        public Task<int> GetTransactionCount(Guid id, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default) =>
            DataContext.Transactions.Where((t) => t.AccountId == id && (filter == null || (t.Description != null && t.Description.Contains(filter))) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end)).CountAsync(cancellationToken);

        public Task<int> GetUntaggedTransactionCount(Guid id, string filter, DateTime? start, DateTime? end, CancellationToken cancellationToken = default) =>
            DataContext.Transactions.Where((t) => t.AccountId == id && !t.TransactionTags.Any() && (filter == null || (t.Description != null && t.Description.Contains(filter))) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end)).CountAsync(cancellationToken);

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default)
        {
            _security.AssertPermission(accountId);

            return (await GetTransactionsQuery(accountId).Where(t => t.AccountId == accountId).ToListAsync(cancellationToken)).OrderByDescending(t => t.TransactionTime);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
        {
            _security.AssertPermission(accountId);

            Expression<Func<Transaction, bool>> predicate = (t) => (filter == null || t.Description.Contains(filter)) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end);

            return await GetTransactionsQuery(accountId).Where(predicate).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, DateTime start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= start && t.TransactionTime <= (end ?? DateTime.Now)).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, TimeSpan period, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default)
        {
            _security.AssertPermission(accountId);

            return await GetTransactionsQuery(accountId).Where(t => t.TransactionTime >= DateTime.Now.Subtract(period) && t.TransactionTime <= DateTime.Now).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToListAsync(cancellationToken);
        }

        private IQueryable<Transaction> GetTransactionsQuery(Guid accountId)
        {
            return DataSet.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId);
        }

        public void AddRange(IEnumerable<Transaction> transactions)
        {
            DataContext.AddRange(transactions);
        }

        public async Task<IEnumerable<Transaction>> GetUntaggedTransactions(Guid accountId, string filter, DateTime? start, DateTime? end, int pageSize, int pageNumber, string sortField, SortDirection sortDirection, CancellationToken cancellationToken = default) =>
            await GetTransactionsQuery(accountId).Where((t) => (filter == null || (t.Description != null && t.Description.Contains(filter))) && (start == null || t.TransactionTime >= start) && (end == null || t.TransactionTime <= end)).Where(t => !t.TransactionTags.Any()).Sort(sortField, sortDirection).Page(pageSize, pageNumber).ToListAsync(cancellationToken);

        protected override IQueryable<Transaction> GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public static class IQueryableExtensions
    {
        private static readonly PropertyInfo[] _transactionProperties;

        static IQueryableExtensions()
        {
            _transactionProperties = typeof(Transaction).GetProperties();
        }

        public static IQueryable<Transaction> Page(this IQueryable<Transaction> query, int pageSize, int pageNumber)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public static IOrderedQueryable<Transaction> Sort(this IQueryable<Transaction> query, string field, SortDirection direction)
        {
            if (!String.IsNullOrWhiteSpace(field))
            {
                PropertyInfo property = _transactionProperties.SingleOrDefault(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase));

                if (property == null) throw new ArgumentException($"Unknown field {field}", nameof(field));


                ParameterExpression param = Expression.Parameter(typeof(Transaction), String.Empty);
                MemberExpression propertyExp = Expression.Property(param, field);
                LambdaExpression sort = Expression.Lambda(propertyExp, param);
                MethodCallExpression call = Expression.Call(typeof(Queryable), "OrderBy" + (direction == SortDirection.Descending ? "Descending" : String.Empty), new[] { typeof(Transaction), propertyExp.Type },
                query.Expression,
                Expression.Quote(sort));
                return (IOrderedQueryable<Transaction>)query.Provider.CreateQuery<Transaction>(call);
            }

            Expression<Func<Transaction, object>> sortFunc = t => t.TransactionTime;
            return direction == SortDirection.Ascending ? query.OrderBy(sortFunc) : query.OrderByDescending(sortFunc);

        }
    }
}
