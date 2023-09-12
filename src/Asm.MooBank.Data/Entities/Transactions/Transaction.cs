using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
public partial class Transaction
{
    public Transaction()
    {
        Tags = new HashSet<Tag.Tag>();
    }

    public Guid TransactionId { get; set; }

    public Guid? TransactionReference { get; set; }

    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public decimal NetAmount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionTime { get; set; }

    public string? Notes { get; set; }

    public bool ExcludeFromReporting { get; set; }

    public Guid? OffsetByTransactionId { get; set; }

    public virtual ICollection<Tag.Tag> Tags { get; set; }

    public virtual Account.Account Account { get; set; }

    public virtual ICollection<TransactionOffset> OffsetBy { get; set; } = new HashSet<TransactionOffset>();

    public virtual ICollection<TransactionOffset> Offsets { get; set; } = new HashSet<TransactionOffset>();

    public TransactionType TransactionType { get; set; }

    public void RemoveOffset(TransactionOffset offset)
    {
        OffsetBy.Remove(offset);
    }
}
