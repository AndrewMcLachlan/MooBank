namespace Asm.MooBank.Domain.Entities;

public partial class Transaction
{
    public Transaction()
    {
        TransactionTags = new HashSet<TransactionTag>();
    }

    public Guid TransactionId { get; set; }

    public Guid? TransactionReference { get; set; }

    public Guid AccountId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionTime { get; set; }

    public virtual ICollection<TransactionTag> TransactionTags { get; set; }

    public virtual Account.Account Account { get; set; }

    public TransactionType? TransactionType { get; set; }
}
