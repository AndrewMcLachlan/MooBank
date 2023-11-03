using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Domain.Entities.Account;

public class TransactionTagRuleTransactionTag
{
    public int TransactionTagRuleId { get; set; }

    public int TransactionTagId { get; set; }

    public virtual Rule TransactionTagRule { get; set; }

    public virtual Tag.Tag TransactionTag { get; set; }
}
