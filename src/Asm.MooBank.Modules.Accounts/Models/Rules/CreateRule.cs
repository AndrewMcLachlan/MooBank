namespace Asm.MooBank.Modules.Accounts.Models.Rules;
public record CreateRule
{
    public required Guid AccountId { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<int> TagIds { get; set; }
}
