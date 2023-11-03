namespace Asm.MooBank.Web.Api.Models;

public record TransactionTagRuleModel
{
    public required string Contains { get; set; }

    public required string Description { get; init; }

    public required IEnumerable<Tag> Tags { get; init; }
}
