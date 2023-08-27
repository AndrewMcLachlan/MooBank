using Asm.Domain;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Transactions;

[AggregateRoot]
public partial class Transaction
{
    public Transaction()
    {
        TransactionTags = new HashSet<Tag.Tag>();
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

    public virtual ICollection<Tag.Tag> TransactionTags { get; set; }

    public virtual Account.Account Account { get; set; }

    public virtual Transaction? OffsetBy { get; set; }

    public virtual Transaction? Offsets { get; set; }

    public TransactionType TransactionType { get; set; }
}
