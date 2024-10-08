﻿namespace Asm.MooBank.Domain.Entities.Tag;

public interface ITagRepository : IDeletableRepository<Tag, int>, IWritableRepository<Tag, int>
{
    void AddSettings(Tag tag);


    Task<IEnumerable<Tag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default);

    Task<Tag> Get(int id, bool includeSubTags = false, CancellationToken cancellationToken = default);
}
