using Asm.Drawing;

namespace Asm.MooBank.Models;

public sealed record Tag
{
    private readonly TagSettings _settings = new();

    public int Id { get; set; }

    public required string Name { get; set; }

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
