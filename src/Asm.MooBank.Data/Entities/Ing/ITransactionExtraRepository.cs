using Asm.MooBank.Domain.Entities.Ing;

namespace Asm.MooBank.Domain.Repositories.Ing;

public interface ITransactionExtraRepository
{
    void AddRange(IEnumerable<TransactionExtra> transactions);
}
