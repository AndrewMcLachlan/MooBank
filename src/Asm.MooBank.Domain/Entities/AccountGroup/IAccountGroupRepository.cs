namespace Asm.MooBank.Domain.Entities.AccountGroup;

public interface IAccountGroupRepository : IDeletableRepository<AccountGroup, Guid>, IWritableRepository<AccountGroup, Guid>
{
}
