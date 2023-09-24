using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;
public class TransactionOffset : Entity
{
    public Guid TransactionSplitId { get; set; }
    public Guid OffsetTransactionId { get; set; }

    public decimal Amount { get; set; }

    public virtual TransactionSplit TransactionSplit { get; set; } = null!;

    public virtual Transaction OffsetByTransaction { get; set; } = null!;
}
