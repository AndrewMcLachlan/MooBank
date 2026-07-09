using Asm.MooBank.Domain.Entities.Utility.Specifications;
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
        var bills = await billsQuery.Specify(new BillDetailsSpecification()).OrderBy(b => b.Id).Page(query.PageSize, query.PageNumber).ToListAsync(cancellationToken);

        return new PagedResult<Bill>
        {
            Total = count,
            Results = bills.ToModel(),
        };
    }
}
