using System.ComponentModel;

namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// The filter and paging criteria handed to the get-bills tool.
/// </summary>
/// <remarks>
/// A record of its own rather than the <see cref="Queries.Bills.GetAll"/> query it maps onto,
/// because that query makes page size and page number required. That suits the HTTP endpoint,
/// where the caller is a screen that knows what it is showing, but it makes a caller that only
/// wants to look something up choose paging before it can ask anything.
///
/// It is a single concrete type for the reason set out on <see cref="BillImport"/>: a parameter
/// the dependency injection container will claim is dropped from the published schema and
/// supplied by the container instead, and a bare collection is claimed every time.
/// </remarks>
public record BillQuery
{
    [Description("Only return bills issued on or after this date. Omit for no lower bound.")]
    public DateOnly? StartDate { get; init; }

    [Description("Only return bills issued on or before this date. Omit for no upper bound.")]
    public DateOnly? EndDate { get; init; }

    [Description("Only return bills for the utility account with this id, as returned by get-bill-accounts. Omit to cover every account.")]
    public Guid? AccountId { get; init; }

    [Description("Only return bills for accounts of this utility type (Electricity, Gas, Water, Phone, Internet or Other). Omit to cover every type.")]
    public UtilityType? UtilityType { get; init; }

    [Description("How many bills to return in one page. Defaults to 20.")]
    public int PageSize { get; init; } = 20;

    [Description("Which page to return, counting from 1. Defaults to the first page, which holds the most recently issued bills.")]
    public int PageNumber { get; init; } = 1;
}
