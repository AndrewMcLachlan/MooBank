using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Domain.Entities.Account;

public partial class TransactionTagRule
{

    public TransactionTagRule()
    {
        TransactionTags = new HashSet<TransactionTag>();
    }
    public int TransactionTagRuleId { get; set; }

    public Guid AccountId { get; set; }

    public string Contains { get; set; }

    public virtual Account Account { get; set; }

    public virtual ICollection<TransactionTag> TransactionTags { get; set; }
}
