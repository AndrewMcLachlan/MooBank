namespace Asm.MooBank.Web.Models;

public record TransactionTagRuleModel
{
    public required string Contains { get; set; }

    public required string Description { get; init; }

    public required IEnumerable<TransactionTag> Tags { get; init; }
}
