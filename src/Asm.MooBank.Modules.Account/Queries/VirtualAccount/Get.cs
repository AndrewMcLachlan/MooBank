﻿using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;

public record Get(Guid AccountId, Guid VirtualAccountId) : IQuery<Models.Account.VirtualAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Account> accounts, AccountHolder accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Account.VirtualAccount>
{
    private readonly IQueryable<Domain.Entities.Account.Account> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.VirtualAccounts).ThenInclude(va => va.RecurringTransactions).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount) throw new InvalidOperationException("Virtual accounts are only available for institution accounts.");

        var virtualAccount = institutionAccount.VirtualAccounts.SingleOrDefault(va => va.Id == request.VirtualAccountId) ?? throw new NotFoundException();

        return virtualAccount.ToModel();

    }
}
