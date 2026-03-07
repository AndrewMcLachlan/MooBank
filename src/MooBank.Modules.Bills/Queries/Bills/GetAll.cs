using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record GetAll : IQuery<PagedResult<Models.Bill>>
{
    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public Guid? AccountId { get; init; }

    public UtilityType? UtilityType { get; init; }
}

internal class GetAllHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetAll, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var billsQuery = accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.ServiceCharge)
            .Include(b => b.Periods)
            .ThenInclude(p => p.Usage)
            .Include(b => b.Discounts)
            .AsQueryable();

        if (query.StartDate.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.IssueDate >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.IssueDate <= query.EndDate.Value);
        }

        if (query.AccountId.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.AccountId == query.AccountId.Value);
        }

        if (query.UtilityType.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.Account.UtilityType == query.UtilityType.Value);
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
