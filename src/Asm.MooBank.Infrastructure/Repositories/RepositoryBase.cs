using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected BankPlusContext DataContext { get; }

    protected IQueryable<TEntity> DataSet
    {
        get => DataContext.Set<TEntity>();
    }

    protected RepositoryBase(BankPlusContext dataContext)
    {
        DataContext = dataContext;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default) => await DataSet.ToListAsync(cancellationToken);

    public async Task<TEntity> Get(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await GetById(id).SingleOrDefaultAsync(cancellationToken);

        return entity ?? throw new NotFoundException();
    }

    public virtual TEntity Add(TEntity entity)
    {
        return DataContext.Set<TEntity>().Add(entity).Entity;
    }

    public TEntity Update(TEntity entity)
    {
        return DataContext.Set<TEntity>().Update(entity).Entity;
    }

    protected abstract IQueryable<TEntity> GetById(TKey id);

}
