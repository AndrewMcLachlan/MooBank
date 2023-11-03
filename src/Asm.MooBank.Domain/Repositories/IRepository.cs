namespace Asm.MooBank.Domain.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);

    Task<TEntity> Get(TKey id, CancellationToken cancellationToken = default);
}
