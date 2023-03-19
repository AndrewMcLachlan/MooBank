namespace Asm.MooBank.Domain.Repositories;

public interface IDeletableRepository<TEntity, TKey> : IWritableRepository<TEntity, TKey> where TEntity : class
{
    void Delete(TEntity entity);
}
