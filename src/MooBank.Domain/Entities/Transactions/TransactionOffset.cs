using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;

[Table("TransactionSplitOffset", Schema = "dbo")]
[PrimaryKey(nameof(TransactionSplitId), nameof(OffsetTransactionId))]
public partial class TransactionOffset : Entity
{
    public Guid TransactionSplitId { get; set; }

    public Guid OffsetTransactionId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    [Navigation]
    public virtual partial TransactionSplit TransactionSplit { get; set; }

    [Navigation]
    public virtual partial Transaction OffsetByTransaction { get; set; }
}
