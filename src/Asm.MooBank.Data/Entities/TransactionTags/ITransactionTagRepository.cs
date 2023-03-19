using Asm.MooBank.Domain.Entities;


namespace Asm.MooBank.Domain.Repositories;

public interface ITransactionTagRepository : IDeletableRepository<TransactionTag, int>
{
    Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds);

    Task<TransactionTag> Get(int id, bool includeSubTags = false);
}
