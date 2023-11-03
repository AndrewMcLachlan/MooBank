using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IAccountRepository : IDeletableRepository<Account, Guid>
{
}
