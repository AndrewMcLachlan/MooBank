using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Infrastructure.Repositories.Ing
{
    public class TransactionExtraRepository : RepositoryBase<TransactionExtra, Guid>, ITransactionExtraRepository
    {
        public TransactionExtraRepository(MooBankContext dataContext) : base(dataContext)
        {
        }

        public void AddRange(IEnumerable<TransactionExtra> transactions)
        {
            DataContext.AddRange(transactions);
        }

        public async Task<IEnumerable<Transaction>> GetUnprocessedTransactions(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default)
        {
            var ids = await DataSet.Select(t => t.TransactionId).ToListAsync(cancellationToken);

            return transactions.Where(t => !ids.Contains(t.TransactionId));
        }

        public async Task<IEnumerable<TransactionExtra>> GetAll(Guid accountId, CancellationToken cancellationToken = default) =>
            await DataSet.Where(t => t.Transaction.AccountId == accountId).ToListAsync(cancellationToken);

        protected override IQueryable<TransactionExtra> GetById(Guid id) => DataSet.Where(t => t.TransactionId == id);
    }
}
