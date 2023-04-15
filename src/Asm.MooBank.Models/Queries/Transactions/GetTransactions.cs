namespace Asm.MooBank.Models.Queries.Transactions;

public record GetTransactions : IQuery<PagedResult<Transaction>>
{
    public required Guid AccountId { get; init; }

    public string? Filter { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public int? TagId { get; set; }

    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public string? SortField { get; init; }

    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    public required bool UntaggedOnly { get; init; } = false;
}
