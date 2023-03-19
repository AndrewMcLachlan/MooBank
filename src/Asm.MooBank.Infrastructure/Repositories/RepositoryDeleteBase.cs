using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryDeleteBase<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IDeletableRepository<TEntity, TKey> where TEntity : class
{

    protected RepositoryDeleteBase(BankPlusContext dataContext) : base(dataContext)
    {
    }

    public virtual void Delete(TEntity entity)
    {
        DataContext.Set<TEntity>().Remove(entity);
    }
}
