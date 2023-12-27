using Asm.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryDeleteBase<TEntity, TKey> : Asm.Domain.Infrastructure.RepositoryDeleteBase<MooBankContext, TEntity, TKey> where TEntity : KeyedEntity<TKey> where TKey : struct
{

    protected RepositoryDeleteBase(MooBankContext dataContext) : base(dataContext)
    {
    }

    public override void Delete(TKey id)
    {
        throw new NotImplementedException();
    }

    protected abstract IQueryable<TEntity> GetById(TKey id);

}
