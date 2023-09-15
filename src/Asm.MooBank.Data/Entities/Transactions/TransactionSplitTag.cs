using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;

public partial class TransactionSplitTag : Entity
{
    public Guid TransactionSplitId { get; set; }

    public int TagId { get; set; }

    public virtual TransactionSplit TransactionSplit { get; set; } = null!;

    public virtual Tag.Tag Tag { get; set; } = null!;
}
