using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionTagRepository(MooBankContext dataContext, Models.AccountHolder accountHolder) : RepositoryDeleteBase<Tag, int>(dataContext), ITagRepository
{
    public void AddSettings(Tag transactionTag)
    {
        if (transactionTag.Settings == null)
        {
            DataContext.Add(new TagSettings(transactionTag.Id));
        }
    }


    public override async Task<IEnumerable<Tag>> GetAll(CancellationToken cancellationToken = default)
    {
        return await DataSet.Include(t => t.Tags).Where(t => t.FamilyId == accountHolder.FamilyId && !t.Deleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default) =>
        await DataSet.Where(t => t.FamilyId == accountHolder.FamilyId && tagIds.Contains(t.Id)).ToListAsync(cancellationToken);


    public async Task<Tag> Get(int id, bool includeSubTags = false, CancellationToken cancellationToken = default)
    {
        var tag = includeSubTags ?
            await GetById(id).Include(t => t.Settings).Include(t => t.Tags).SingleOrDefaultAsync(cancellationToken) :
            await GetById(id).Include(t => t.Settings).SingleOrDefaultAsync(cancellationToken);

        return tag ?? throw new NotFoundException($"Transaction tag with id {id} was not found");
    }

    public override void Delete(Tag tag)
    {
        tag.Deleted = true;
    }

    protected override IQueryable<Tag> GetById(int id) => DataSet.Where(t => t.Id == id && t.FamilyId == accountHolder.FamilyId);
}
