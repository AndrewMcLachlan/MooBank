namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryDeleteBase<TEntity, TKey>(MooBankContext dataContext) : RepositoryDeleteBase<MooBankContext, TEntity, TKey>(dataContext) where TEntity : KeyedEntity<TKey> where TKey : struct
{
    protected abstract override IQueryable<TEntity> GetById(TKey id);
}
