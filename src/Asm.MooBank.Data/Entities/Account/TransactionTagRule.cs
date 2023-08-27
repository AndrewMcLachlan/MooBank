using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class TransactionTagRule
{

    public TransactionTagRule()
    {
        TransactionTags = new HashSet<Tag.Tag>();
    }
    public int TransactionTagRuleId { get; set; }

    public string? Description { get; set; }

    public Guid AccountId { get; set; }

    public string Contains { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<Tag.Tag> TransactionTags { get; set; }
}
