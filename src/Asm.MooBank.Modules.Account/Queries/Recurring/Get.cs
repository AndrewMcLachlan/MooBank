﻿using Asm.MooBank.Modules.Account.Models.Recurring;

namespace Asm.MooBank.Modules.Account.Queries.Recurring;

public record Get(Guid AccountId, Guid RecurringTransactionId) : IQuery<RecurringTransaction>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Account> accounts, ISecurity security) : IQueryHandler<Get, RecurringTransaction>
{

    public async ValueTask<RecurringTransaction> Handle(Get query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var account = await accounts.Include(a => a.VirtualAccounts).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        return account.VirtualAccounts.SelectMany(v => v.RecurringTransactions).SingleOrDefault(r => r.Id == query.RecurringTransactionId)?.ToModel() ?? throw new NotFoundException();
    }
}