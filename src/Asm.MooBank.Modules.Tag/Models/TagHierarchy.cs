namespace Asm.MooBank.Models;

public record TagHierarchy
{
    public required IDictionary<int, int> Levels { get;init; }

    public required IEnumerable<Tag> Tags { get; init; }
}
