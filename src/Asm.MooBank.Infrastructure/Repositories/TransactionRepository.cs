using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Transactions;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository(MooBankContext dataContext) : RepositoryWriteBase<Transaction, Guid>(dataContext), ITransactionRepository
{

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(accountId).ToListAsync(cancellationToken);

    protected override IQueryable<Transaction> GetById(Guid id) =>
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Include(t => t.Splits).ThenInclude(t => t.OffsetBy).ThenInclude(to => to.OffsetByTransaction).Include(t => t.OffsetFor).ThenInclude(to => to.TransactionSplit).ThenInclude(ts => ts.Transaction).Where(t => t.Id == id);

    private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == accountId);
}