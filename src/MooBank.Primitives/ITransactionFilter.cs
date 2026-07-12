namespace Asm.MooBank;

/// <summary>
/// The transaction filtering criteria consumed by the domain's transaction filter specification.
/// The API filter model implements this so the specification depends on an abstraction, not the DTO
/// (mirrors the <c>ISortable</c> pattern used by the sort specification).
/// </summary>
public interface ITransactionFilter
{
    Guid InstrumentId { get; }

    string? Filter { get; }

    DateTime? Start { get; }

    DateTime? End { get; }

    int[]? TagIds { get; }

    TransactionFilterType? TransactionType { get; }

    bool? UntaggedOnly { get; }

    bool? ExcludeNetZero { get; }
}
