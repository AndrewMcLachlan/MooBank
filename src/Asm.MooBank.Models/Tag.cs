using System.ComponentModel;
using Asm.Drawing;

namespace Asm.MooBank.Models;

[DisplayName("SimpleTag")]
public record TagBase
{
    public required int Id { get; set; }

    public required string Name { get; set; }
}

public sealed record Tag : TagBase
{
    private readonly TagSettings _settings = new();

    public HexColour? Colour { get; set; }

    public IEnumerable<Tag> Tags { get; set; } = [];

    public TagSettings Settings
    {
        get => _settings;
        init
        {
            _settings = value ?? new();
        }
    }

    public partial record TagSettings
    {
        public bool ApplySmoothing { get; init; }

        public bool ExcludeFromReporting { get; init; }
    }
}
