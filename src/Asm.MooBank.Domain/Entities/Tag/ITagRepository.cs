using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Tag;

public interface ITagRepository : IDeletableRepository<Tag, int>
{
    void AddSettings(Tag transactionTag);

    Task<IEnumerable<Tag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default);

    Task<Tag> Get(int id, bool includeSubTags = false, CancellationToken cancellationToken = default);
}
