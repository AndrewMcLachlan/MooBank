using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record class GetForAccount(Guid InstrumentId, int PageSize = 20, int PageNumber = 1) : IQuery<PagedResult<Bill>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Utility.Account> accounts) : IQueryHandler<GetForAccount, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetForAccount query, CancellationToken cancellationToken)
    {
        if (!await accounts.AnyAsync(a => a.Id == query.InstrumentId, cancellationToken)) throw new NotFoundException();

        var billsQuery = accounts.Where(a => a.Id == query.InstrumentId).SelectMany(a => a.Bills);

        var count = await billsQuery.CountAsync(cancellationToken);

        // Page in the query rather than loading every bill into memory.
        var bills = await billsQuery
            .Include(b => b.Periods).ThenInclude(p => p.Usage)
            .Include(b => b.Periods).ThenInclude(p => p.ServiceCharge)
            .Include(b => b.Discounts)
            .OrderBy(b => b.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Bill>
        {
            Total = count,
            Results = bills.ToModel(),
        };
    }
}
