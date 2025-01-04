using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record Get(Guid InstrumentId, int Id) : IQuery<Bill>;

internal class GetHandler(IQueryable<Domain.Entities.Utility.Bill> bills) : IQueryHandler<Get, Bill>
{
    public async ValueTask<Bill> Handle(Get query, CancellationToken cancellationToken)
    {
        var bill = await bills.Where(b => b.AccountId == query.InstrumentId && b.Id == query.Id).FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Bill not found");
        return bill.ToModel();
    }
}
