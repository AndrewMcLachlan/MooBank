using Asm.MooBank.Domain.Entities.Utility.Specifications;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record Get(Guid InstrumentId, int Id) : IQuery<Bill>;

internal class GetHandler(IQueryable<Domain.Entities.Utility.Account> accounts) : IQueryHandler<Get, Bill>
{
    public async ValueTask<Bill> Handle(Get query, CancellationToken cancellationToken)
    {
        if (!await accounts.AnyAsync(a => a.Id == query.InstrumentId, cancellationToken)) throw new NotFoundException("Account not found");

        var bill = await accounts.Where(a => a.Id == query.InstrumentId).SelectMany(a => a.Bills).Specify(new BillDetailsSpecification()).FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken) ?? throw new NotFoundException("Bill not found");

        return bill.ToModel();
    }
}
