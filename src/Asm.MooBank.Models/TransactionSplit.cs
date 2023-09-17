namespace Asm.MooBank.Models;
public record TransactionSplit
{
    public required Guid Id { get; init; }

    public required IEnumerable<Tag> Tags { get; init; }

    public required decimal Amount { get; init; }
}
