namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class RepositoryDeleteBase<TEntity, TKey> : RepositoryDeleteBase<MooBankContext, TEntity, TKey> where TEntity : KeyedEntity<TKey> where TKey : struct
{

    protected RepositoryDeleteBase(MooBankContext dataContext) : base(dataContext)
    {
    }

    /// <summary>
    /// Deletes an entity by its key.
    /// </summary>
    /// <remarks>
    /// WARNING: This base implementation always throws. Derived repositories that support
    /// deletion by key must override this method; otherwise callers should use
    /// <see cref="Asm.Domain.IDeletableRepository{TEntity, TKey}.Delete(TEntity)"/> with a loaded entity.
    /// </remarks>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <exception cref="NotImplementedException">Always thrown by this base implementation.</exception>
    public override void Delete(TKey id)
    {
        throw new NotImplementedException();
    }

    protected abstract override IQueryable<TEntity> GetById(TKey id);

}
