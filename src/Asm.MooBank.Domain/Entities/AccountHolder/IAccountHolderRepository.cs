namespace Asm.MooBank.Domain.Entities.AccountHolder;

public interface IAccountHolderRepository : IRepository<AccountHolder, Guid>
{

    //Task<AccountHolder?> GetCurrentOrNull(CancellationToken cancellationToken = default);

    //Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default);

    Task<AccountHolder?> GetByCard(short last4Digits, CancellationToken cancellationToken = default);
}
