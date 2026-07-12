namespace Asm.MooBank;

/// <summary>
/// The desired state of a transaction split, consumed by the <c>Transaction</c> aggregate when it
/// reconciles its splits. The API split model implements this (explicitly) so the aggregate depends
/// on an abstraction rather than the DTO, and no new members are added to the serialized contract.
/// </summary>
public interface ITransactionSplitUpdate
{
    Guid Id { get; }

    decimal Amount { get; }

    IEnumerable<int> TagIds { get; }

    IEnumerable<ITransactionOffsetUpdate> OffsetBy { get; }
}

/// <summary>
/// The desired state of an offset applied to a split, expressed as the offsetting transaction's id
/// and amount.
/// </summary>
public interface ITransactionOffsetUpdate
{
    Guid OffsetTransactionId { get; }

    decimal Amount { get; }
}
