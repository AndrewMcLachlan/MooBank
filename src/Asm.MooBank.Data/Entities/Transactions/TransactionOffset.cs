using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;
public class TransactionOffset : Entity
{
    public Guid TransactionId { get; set; }
    public Guid OffsetTransactionId { get; set; }

    public decimal Amount { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual Transaction OffsetByTransaction { get; set; } = null!;
}
