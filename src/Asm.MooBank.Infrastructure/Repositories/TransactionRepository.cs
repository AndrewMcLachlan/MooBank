﻿using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Events;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionRepository(MooBankContext dataContext) : Asm.Domain.Infrastructure.RepositoryWriteBase<MooBankContext, Transaction, Guid>(dataContext), ITransactionRepository
{
    public override Transaction Add(Transaction entity)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new TransactionAddedEvent(tracked));
        return tracked;
    }

    public async Task<IEnumerable<Transaction>> GetTransactions(Guid accountId, CancellationToken cancellationToken = default) =>
        await GetTransactionsQuery(accountId).ToListAsync(cancellationToken);

    private IQueryable<Transaction> GetTransactionsQuery(Guid accountId) =>
        Entities.Include(t => t.Splits).ThenInclude(t => t.Tags).Where(t => t.AccountId == accountId);
}
