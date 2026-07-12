namespace Asm.MooBank.Models;

public record TransactionSplit : ITransactionSplitUpdate
{
    public required Guid Id { get; init; }

    public required IEnumerable<TagBase> Tags { get; init; }

    public required decimal Amount { get; init; }

    public IEnumerable<TransactionOffsetBy> OffsetBy { get; set; } = [];

    // Explicit implementations expose the split's desired state to the Transaction aggregate as a
    // domain-native abstraction without adding members to the serialized (OpenAPI) contract.
    IEnumerable<int> ITransactionSplitUpdate.TagIds => Tags.Select(t => t.Id);

    IEnumerable<ITransactionOffsetUpdate> ITransactionSplitUpdate.OffsetBy => OffsetBy;
}
