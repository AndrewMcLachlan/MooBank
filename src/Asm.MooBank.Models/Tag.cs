namespace Asm.MooBank.Models;

public partial record Tag
{
    private readonly TagSettings _settings = new();

    public int Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<Tag> Tags { get; set; } = Enumerable.Empty<Tag>();

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


