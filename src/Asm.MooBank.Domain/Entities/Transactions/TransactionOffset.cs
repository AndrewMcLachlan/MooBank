using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[Table("TransactionSplitOffset", Schema = "dbo")]
[PrimaryKey(nameof(TransactionId), nameof(TransactionSplitId), nameof(OffsetTransactionId))]
public class TransactionOffset : Entity
{
     public Guid TransactionId { get; set; }

     public int TransactionSplitId { get; set; }

     public Guid OffsetTransactionId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    public virtual TransactionSplit TransactionSplit { get; set; } = null!;

    public virtual Transaction OffsetByTransaction { get; set; } = null!;
}
