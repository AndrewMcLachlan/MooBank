using Asm.Cqrs.Queries;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record GetAll : IQuery<PagedResult<Models.Bill>>
{
    public required int PageSize { get; init; }

    public required int PageNumber { get; init; }
}

internal class GetAllHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetAll, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var count = await accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId)).SelectMany(a => a.Bills).CountAsync(cancellationToken);
        var bills = await accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.ServiceCharge)
            .Include(b => b.Periods)
            .ThenInclude(p => p.Usage)
            .OrderByDescending(a => a.IssueDate)
            .Page(query.PageSize, query.PageNumber).ToListAsync(cancellationToken);

        return new PagedResult<Bill>
        {
            Results = bills.ToModel(),
            Total = count,
        };
    }
}
