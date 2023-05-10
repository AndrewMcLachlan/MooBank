using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Security;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository : RepositoryBase<Transaction, Guid>, ITransactionRepository
{
    private readonly ISecurity _security;

    public TransactionRepository(MooBankContext dataContext, ISecurity security) : base(dataContext)
    {
        _security = security;
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(accountId).ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<Transaction> transactions)
    {
        DataContext.AddRange(transactions);
    }

    protected override IQueryable<Transaction> GetById(Guid id) =>
        DataSet.Where(t => t.TransactionId == id);

    private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
        DataSet.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId);
}