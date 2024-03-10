using Asm.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity, TKey>(MooBankContext context) : Asm.Domain.Infrastructure.RepositoryBase<MooBankContext, TEntity, TKey>(context), IRepository<TEntity, TKey> where TEntity : KeyedEntity<TKey> where TKey : struct
{
    protected abstract IQueryable<TEntity> GetById(TKey id);

}
