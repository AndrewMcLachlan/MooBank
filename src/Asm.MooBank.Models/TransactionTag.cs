namespace Asm.MooBank.Models;

public partial record TransactionTag
{
    private readonly TransactionTagSettings _settings = new();

    public int Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<TransactionTag> Tags { get; set; } = Enumerable.Empty<TransactionTag>();

    public TransactionTagSettings Settings
    {
        get => _settings;
        init
        {
            _settings = value ?? new();
        }
    }

    public partial record TransactionTagSettings
    {
        public bool ApplySmoothing { get; init; }

        public bool ExcludeFromReporting { get; init; }
    }
}


