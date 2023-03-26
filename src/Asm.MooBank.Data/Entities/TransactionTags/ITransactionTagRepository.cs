using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.TransactionTags;

public interface ITransactionTagRepository : IDeletableRepository<TransactionTag, int>
{
    Task<IEnumerable<TransactionTag>> Get(IEnumerable<int> tagIds);

    Task<TransactionTag> Get(int id, bool includeSubTags = false);
}
