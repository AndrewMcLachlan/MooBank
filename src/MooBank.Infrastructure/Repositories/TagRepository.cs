using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.Repositories;

internal sealed class TagRepository(MooBankContext dataContext) : RepositoryDeleteBase<MooBankContext, Tag, int>(dataContext), ITagRepository
{
    // Family and soft-delete scoping are applied by the named query filters on Tag.

    public override async Task<IEnumerable<Tag>> Get(CancellationToken cancellationToken = default)
    {
        return await Entities.Include(t => t.Tags).ToListAsync(cancellationToken);
    }

    public override Task<Tag> Get(int id, CancellationToken cancellationToken = default)
    {
        return Get(id, false, cancellationToken);
    }

    public async Task<IEnumerable<Tag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default) =>
        await Entities.Where(t => tagIds.Contains(t.Id)).ToListAsync(cancellationToken);


    public async Task<Tag> Get(int id, bool includeSubTags = false, CancellationToken cancellationToken = default)
    {
        var tag = includeSubTags ?
            await GetById(id).Include(t => t.Settings).Include(t => t.Tags).SingleOrDefaultAsync(cancellationToken) :
            await GetById(id).Include(t => t.Settings).SingleOrDefaultAsync(cancellationToken);

        return tag ?? throw new NotFoundException($"Transaction tag with id {id} was not found");
    }

    public override void Delete(int id)
    {
        var tag = GetById(id).SingleOrDefault() ?? throw new NotFoundException($"Transaction tag with id {id} was not found");
        Delete(tag);
    }

    public override void Delete(Tag tag)
    {
        tag.Deleted = true;
    }

    protected override IQueryable<Tag> GetById(int id) => Entities.Where(t => t.Id == id);
}
