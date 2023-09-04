namespace Asm.MooBank.Models;

public record TransactionTagHierarchy
{
    public required IDictionary<int, int> Levels { get;init; }

    public required IEnumerable<Tag> Tags { get; init; }
}
