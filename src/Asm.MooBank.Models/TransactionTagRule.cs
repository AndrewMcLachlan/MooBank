namespace Asm.MooBank.Models;

public partial record TransactionTagRule
{
    public required int Id { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<TransactionTag> Tags { get; set; }
}
