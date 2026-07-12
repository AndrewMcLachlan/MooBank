namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

/// <summary>
/// Domain-native criteria consumed by <see cref="FilterSpecification"/>. The application layer
/// translates the API <c>TransactionFilter</c> DTO into this type so the specification does not
/// depend on the Models contract.
/// </summary>
public record TransactionFilterCriteria
{
    public required Guid InstrumentId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public int[]? TagIds { get; init; }

    public TransactionFilterType? TransactionType { get; init; }

    public bool? UntaggedOnly { get; init; }

    public bool? ExcludeNetZero { get; init; }
}
