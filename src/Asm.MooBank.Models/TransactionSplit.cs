namespace Asm.MooBank.Models;

public record TransactionSplit
{
    public required int Id { get; init; }

    public required IEnumerable<Tag> Tags { get; init; }

    public required decimal Amount { get; init; }

    public IEnumerable<TransactionOffsetBy> OffsetBy { get; set; } = [];
}
