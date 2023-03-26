namespace Asm.MooBank.Models;

public partial record AccountGroup
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool ShowPosition { get; init; }
}
