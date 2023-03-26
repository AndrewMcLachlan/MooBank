namespace Asm.MooBank.Domain.Entities.Ing;

public interface ITransactionExtraRepository
{
    void AddRange(IEnumerable<TransactionExtra> transactions);
}
