namespace Asm.MooBank.Domain.Entities.Transactions;

/// <summary>
/// Domain-native input describing the desired state of a transaction split. The application layer
/// translates the API <c>TransactionSplit</c> DTO into this type so the <see cref="Transaction"/>
/// aggregate does not depend on the Models contract.
/// </summary>
public record SplitUpdate
{
    public required Guid Id { get; init; }

    public required decimal Amount { get; init; }

    public required IEnumerable<int> TagIds { get; init; }

    public IEnumerable<OffsetUpdate> OffsetBy { get; init; } = [];
}

/// <summary>
/// Domain-native input describing an offset applied to a split, expressed as the offsetting
/// transaction's id and amount.
/// </summary>
public record OffsetUpdate
{
    public required Guid TransactionId { get; init; }

    public required decimal Amount { get; init; }
}
