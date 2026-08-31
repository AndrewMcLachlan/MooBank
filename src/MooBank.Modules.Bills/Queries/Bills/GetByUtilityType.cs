using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record GetByUtilityType : IQuery<PagedResult<Models.Bill>>
{
    public required UtilityType UtilityType { get; init; }

    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public Guid? AccountId { get; init; }

    /// <summary>
    /// A period named rather than dated. Set, it selects that bill and the dates are ignored.
    /// </summary>
    public BillPeriod? Period { get; init; }
}

internal class GetByUtilityTypeHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetByUtilityType, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetByUtilityType query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var billsQuery = accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .Where(a => a.UtilityType == query.UtilityType)
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.ServiceCharges)
            .ThenInclude(sc => sc.ChargeType)
            .Include(b => b.Periods)
            .ThenInclude(p => p.Usages)
            .Include(b => b.Discounts)
            .AsQueryable();

        // Dates only narrow when no period is named: a named period is the request, not a range to
        // be narrowed further, and applying both would let a stale range empty it.
        if (!query.Period.HasValue)
        {
            if (query.StartDate.HasValue)
            {
                billsQuery = billsQuery.Where(b => b.IssueDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                billsQuery = billsQuery.Where(b => b.IssueDate <= query.EndDate.Value);
            }
        }

        if (query.AccountId.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.AccountId == query.AccountId.Value);
        }

        /*
            A named period resolves here, where the bills are, rather than at the caller: asking for
            "the last period" should not require knowing which dates that covers, and a caller that
            had to fetch bills to find out would be filtering by the very thing it was asking for.

        */
        if (query.Period.HasValue)
        {
            var skip = query.Period.Value == BillPeriod.Previous ? 1 : 0;

            var billId = await billsQuery
                .OrderByDescending(b => b.IssueDate).ThenByDescending(b => b.Id)
                .Skip(skip).Take(1)
                .Select(b => (int?)b.Id)
                .SingleOrDefaultAsync(cancellationToken);

            // No such bill -- a new account, or only one when the previous was asked for.
            billsQuery = billsQuery.Where(b => billId != null && b.Id == billId);
        }

        var count = await billsQuery.CountAsync(cancellationToken);
        var bills = await billsQuery
            .OrderByDescending(a => a.IssueDate)
            .Page(query.PageSize, query.PageNumber)
            .ToListAsync(cancellationToken);

        return new PagedResult<Bill>
        {
            Results = bills.ToModel(),
            Total = count,
        };
    }
}
