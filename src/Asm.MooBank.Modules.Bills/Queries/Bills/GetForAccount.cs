using Asm.MooBank.Domain.Entities.Utility.Specifications;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record class GetForAccount(Guid InstrumentId, int PageSize = 20, int PageNumber = 1) : IQuery<PagedResult<Bill>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Utility.Account> accounts) : IQueryHandler<GetForAccount, PagedResult<Bill>>
{
    public async ValueTask<PagedResult<Bill>> Handle(GetForAccount query, CancellationToken cancellationToken)
    {
        var account = await accounts.Specify(new BillDetailsSpecification()).Where(a => a.Id == query.InstrumentId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var count = account.Bills.Count;
        var all = account.Bills.Page(query.PageSize, query.PageNumber).ToList();

        return new PagedResult<Bill>
        {
            Total = count,
            Results = all.ToModel(),
        };
    }
}
