using System.ComponentModel;
using Asm.Drawing;

namespace Asm.MooBank.Modules.Tags.Models;

public record TagGraph
{
    public required IReadOnlyList<TagNode> Nodes { get; init; }
    public required IReadOnlyList<TagEdge> Edges { get; init; }
}

[DisplayName("TagGraphNode")]
public record TagNode
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public HexColour? Colour { get; init; }
    public required TagNodeSettings Settings { get; init; }
}

[DisplayName("TagGraphNodeSettings")]
public record TagNodeSettings
{
    public required bool ApplySmoothing { get; init; }
    public required bool ExcludeFromReporting { get; init; }
}

[DisplayName("TagGraphEdge")]
public record TagEdge
{
    public required int ParentId { get; init; }
    public required int ChildId { get; init; }
}
