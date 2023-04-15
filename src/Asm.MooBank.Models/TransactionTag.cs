namespace Asm.MooBank.Models;

public partial record TransactionTag
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public IEnumerable<TransactionTag> Tags { get; set; } = Enumerable.Empty<TransactionTag>();

    public TransactionTagSettings Settings { get; init; } = new();

    public partial record TransactionTagSettings
    {
        public bool ApplySmoothing { get; init; }
    }
}


