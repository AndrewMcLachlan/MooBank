using System.Linq.Expressions;
using System.Reflection;
using Asm.MooBank.Domain.Entities.Transactions;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
    {
        private readonly ISecurityRepository _security;

        public TransactionRepository(MooBankContext dataContext, ISecurityRepository security) : base(dataContext)
        {
            _security = security;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default)
        {
            _security.AssertAccountPermission(accountId);

            return (await GetTransactionsQuery(accountId).ToListAsync(cancellationToken));
        }

        public void AddRange(IEnumerable<Transaction> transactions)
        {
            DataContext.AddRange(transactions);
        }

        protected override IQueryable<Transaction> GetById(Guid id) =>
            DataContext.Transactions.Where(t => t.TransactionId == id);

        private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
            DataSet.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId);
    }
}
