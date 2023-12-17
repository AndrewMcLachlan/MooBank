namespace Asm.MooBank.Domain.Entities.Account;

public class TransactionTagRuleTransactionTag
{
    public int TransactionTagRuleId { get; set; }

    public int TransactionTagId { get; set; }

    public Rule TransactionTagRule { get; set; } = null!;

    public Tag.Tag TransactionTag { get; set; } = null!;
}
