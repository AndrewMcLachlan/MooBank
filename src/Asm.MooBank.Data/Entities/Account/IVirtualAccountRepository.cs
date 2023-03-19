using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IVirtualAccountRepository : IDeletableRepository<VirtualAccount, Guid>
{
}
