﻿using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IAccountRepository : IDeletableRepository<Account, Guid>
{
    Task<InstitutionAccount> GetInstitutionAccount(Guid accountId, CancellationToken cancellationToken);

    Task<VirtualAccount> GetVirtualAccount(Guid accountId, Guid institutionAccountId, CancellationToken cancellationToken);
}
