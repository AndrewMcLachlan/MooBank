using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Transactions;

public partial class TransactionSplitTag : Entity
{
    public Guid TransactionSplitId { get; set; }

    public int TagId { get; set; }

    [Navigation]
    public virtual partial TransactionSplit TransactionSplit { get; set; }

    [Navigation]
    public virtual partial Tag.Tag Tag { get; set; }
}
