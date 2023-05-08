using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionTagRepository : RepositoryDeleteBase<TransactionTag, int>, ITransactionTagRepository
{
    public TransactionTagRepository(BankPlusContext dataContext) : base(dataContext)
    {
    }

    public void AddSettings(TransactionTag transactionTag)
    {
        if (transactionTag.Settings == null)
        {
            DataContext.Add(new TransactionTagSettings(transactionTag.TransactionTagId));
        }
    }


    public override async Task<IEnumerable<TransactionTag>> GetAll(CancellationToken cancellationToken = default)
    {
        return await DataSet.Include(t => t.Tags).Where(t => !t.Deleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds, CancellationToken cancellationToken = default) =>
        await DataSet.Where(t => tagIds.Contains(t.TransactionTagId)).ToListAsync(cancellationToken);


    public async Task<TransactionTag> Get(int id, bool includeSubTags = false, CancellationToken cancellationToken = default)
    {
        var tag = includeSubTags ?
            await GetById(id).Include(t => t.Settings).Include(t => t.Tags).SingleOrDefaultAsync(cancellationToken) :
            await GetById(id).Include(t => t.Settings).SingleOrDefaultAsync(cancellationToken);

        return tag ?? throw new NotFoundException($"Transaction tag with id {id} was not found");
    }

    public override void Delete(TransactionTag tag)
    {
        tag.Deleted = true;
    }

    protected override IQueryable<TransactionTag> GetById(int id) => DataSet.Where(t => t.TransactionTagId == id);
}
