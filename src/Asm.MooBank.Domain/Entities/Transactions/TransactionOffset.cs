using Asm.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions;
public class TransactionOffset : Entity
{
    public Guid TransactionSplitId { get; set; }
    public Guid OffsetTransactionId { get; set; }

    [Precision(12, 4)]
    public decimal Amount { get; set; }

    public virtual TransactionSplit TransactionSplit { get; set; } = null!;

    public virtual Transaction OffsetByTransaction { get; set; } = null!;
}
