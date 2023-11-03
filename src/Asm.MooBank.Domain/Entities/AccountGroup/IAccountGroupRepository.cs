using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.AccountGroup;

public interface IAccountGroupRepository : IDeletableRepository<AccountGroup, Guid>
{
}
