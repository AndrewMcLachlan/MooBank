using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record class GetForAccount(Guid InstrumentId, int PageSize = 20, int PageNumber = 1) : IQuery<PagedResult<Bill>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Utility.Bill> bills) : IQueryHandler<GetForAccount, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetForAccount query, CancellationToken cancellationToken)
    {
        var count = await bills.Where(b => b.AccountId == query.InstrumentId).CountAsync(cancellationToken);
        var all = await bills.Where(b => b.AccountId == query.InstrumentId).Page(query.PageSize, query.PageNumber).ToListAsync(cancellationToken);
        return new PagedResult<Bill>
        {
            Total = count,
            Results = all.ToModel(),
        };
    }
}
