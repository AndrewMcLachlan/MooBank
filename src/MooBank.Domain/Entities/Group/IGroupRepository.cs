namespace Asm.MooBank.Domain.Entities.Group;

public interface IGroupRepository : IDeletableRepository<Group, Guid>, IWritableRepository<Group, Guid>
{
}
