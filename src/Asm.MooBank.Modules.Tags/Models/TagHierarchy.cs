using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tags.Models;

public record TagHierarchy
{
    public required IDictionary<int, int> Levels { get; init; }

    public required IEnumerable<Tag> Tags { get; init; }
}
