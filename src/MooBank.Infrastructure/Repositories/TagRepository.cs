using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.Repositories;

internal sealed class TagRepository(MooBankContext dataContext, Models.User user) : RepositoryDeleteBase<Tag, int>(dataContext), ITagRepository
{
    public override async Task<IEnumerable<Tag>> Get(CancellationToken cancellationToken = default)
    {
        return await Entities.Include(t => t.Tags).Where(t => t.FamilyId == user.FamilyId && !t.Deleted).ToListAsync(cancellationToken);
    }

    public override Task<Tag> Get(int id, CancellationToken cancellationToken = default)
    {
        return Get(id, false, cancellationToken);
    }

    public async Task<IEnumerable<Tag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default) =>
        await Entities.Where(t => t.FamilyId == user.FamilyId && !t.Deleted && tagIds.Contains(t.Id)).ToListAsync(cancellationToken);


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

    protected override IQueryable<Tag> GetById(int id) => Entities.Where(t => t.Id == id && t.FamilyId == user.FamilyId);
}
