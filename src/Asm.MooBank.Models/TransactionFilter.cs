namespace Asm.MooBank.Models;
public record TransactionFilter : ISortable
{
    private bool _untagged;

    public required Guid InstrumentId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public int[]? TagIds { get; set; }

    public string? SortField { get; init; }

    public TransactionFilterType? TransactionType { get; init; } = TransactionFilterType.None;

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    public string Untagged { init => _untagged = value == "untagged"; }

    public bool? UntaggedOnly => _untagged;
}
