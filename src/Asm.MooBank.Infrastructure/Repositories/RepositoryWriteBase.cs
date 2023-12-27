using Asm.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryWriteBase<TEntity, TKey> : Asm.Domain.Infrastructure.RepositoryWriteBase<MooBankContext, TEntity, TKey>, IRepository<TEntity, TKey> where TEntity : KeyedEntity<TKey> where TKey : struct
{
    protected MooBankContext DataContext { get; }

    protected IQueryable<TEntity> DataSet
    {
        get => DataContext.Set<TEntity>();
    }

    protected RepositoryWriteBase(MooBankContext dataContext) : base(dataContext)
    {
        DataContext = dataContext;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default) => await DataSet.ToListAsync(cancellationToken);

    protected abstract IQueryable<TEntity> GetById(TKey id);

}
