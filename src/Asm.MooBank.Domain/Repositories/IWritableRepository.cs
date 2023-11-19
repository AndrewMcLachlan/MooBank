namespace Asm.MooBank.Domain.Repositories;

public interface IWritableRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{

    TEntity Add(TEntity entity);

    TEntity Update(TEntity entity);
}
