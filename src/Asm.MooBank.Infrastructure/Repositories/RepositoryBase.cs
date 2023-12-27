using Asm.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity, TKey> : Asm.Domain.Infrastructure.RepositoryBase<MooBankContext, TEntity, TKey>, IRepository<TEntity, TKey> where TEntity : KeyedEntity<TKey> where TKey : struct
{
    protected RepositoryBase(MooBankContext context) : base(context)
    {
    }

    protected abstract IQueryable<TEntity> GetById(TKey id);

}
