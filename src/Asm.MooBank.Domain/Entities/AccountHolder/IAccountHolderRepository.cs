using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.AccountHolder;

public interface IAccountHolderRepository : IWritableRepository<AccountHolder, Guid>
{

    //Task<AccountHolder?> GetCurrentOrNull(CancellationToken cancellationToken = default);

    //Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default);

    Task<AccountHolder?> GetByCard(short last4Digits, CancellationToken cancellationToken = default);
}
