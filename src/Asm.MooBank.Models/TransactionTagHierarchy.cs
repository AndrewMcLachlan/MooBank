namespace Asm.MooBank.Models;

public record TransactionTagHierarchy
{
    public required IDictionary<int, int> Levels { get;init; }

    public required IEnumerable<TransactionTag> Tags { get; init; }
}
