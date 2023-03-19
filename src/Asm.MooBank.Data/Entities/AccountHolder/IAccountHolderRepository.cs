using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Domain.Repositories;

public interface IAccountHolderRepository : IWritableRepository<AccountHolder, Guid>
{

    Task<AccountHolder?> GetCurrentOrNull(CancellationToken cancellationToken = default);

    Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default);
}
