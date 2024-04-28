namespace Asm.MooBank.Modules.Instruments.Queries.Rules;
public record CreateRule
{
    public required Guid AccountId { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<int> TagIds { get; set; }
}
