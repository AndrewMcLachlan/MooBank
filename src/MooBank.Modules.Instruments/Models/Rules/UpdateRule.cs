using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Rules;

public record UpdateRule
{
    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<Tag> Tags { get; set; }
}
