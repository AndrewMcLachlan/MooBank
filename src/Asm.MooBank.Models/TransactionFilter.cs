using System.ComponentModel;

namespace Asm.MooBank.Models;

/// <summary>
/// Filter specification for querying transactions.
/// </summary>
[Description("Filter specification for querying transactions.")]
public record TransactionFilter : ISortable
{
    /// <summary>
    /// The ID of the instrument (account) to filter transactions for.
    /// </summary>
    [Description("The ID of the instrument (account) to filter transactions for.")]
    public required Guid InstrumentId { get; init; }

    /// <summary>
    /// The search filter string to apply to transaction descriptions or references.
    /// </summary>
    [Description("The search filter string to apply to transaction descriptions or references.")]
    public string? Filter { get; init; }

    /// <summary>
    /// The date and time to start filtering transactions from.
    /// </summary>
    [Description("The date and time to start filtering transactions from.")]
    public DateTime? Start { get; init; }

    /// <summary>
    /// The date and time to end filtering transactions at.
    /// </summary>
    [Description("The date and time to end filtering transactions at.")]
    public DateTime? End { get; init; }

    /// <summary>
    /// The integer IDs of the tags to filter by.
    /// </summary>
    [Description("The integer IDs of the tags to filter by.")]
    public int[]? TagIds { get; set; }

    /// <summary>
    /// The field to sort the results by.
    /// </summary>
    [Description("The field to sort the results by.")]
    public string? SortField { get; init; }

    /// <summary>
    /// The type of transactions to include.
    /// </summary>
    [Description("The type of transactions to include.")]
    public TransactionFilterType? TransactionType { get; init; } = TransactionFilterType.None;

    /// <summary>
    /// The direction to sort the results.
    /// </summary>
    [Description("The direction to sort the results.")]
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    /// <summary>
    /// Only include untagged transactions.
    /// </summary>
    [Description("Only include untagged transactions.")]
    public bool? UntaggedOnly { get; init; }

    /// <summary>
    /// Exclude transactions where the amount has been offset to zero.
    /// </summary>
    [Description("Exclude transactions where the amount has been offset to zero.")]
    public bool? ExcludeNetZero { get; init; }
}
