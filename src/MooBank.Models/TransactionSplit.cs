namespace Asm.MooBank.Models;

public record TransactionSplit
{
    public required Guid Id { get; init; }

    public required IEnumerable<TagBase> Tags { get; init; }

    public required decimal Amount { get; init; }

    public IEnumerable<TransactionOffsetBy> OffsetBy { get; set; } = [];
}
