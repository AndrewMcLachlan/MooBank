using Asm.MooBank.Domain.Entities.Utility.Specifications;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Queries.Bills;

public record Get(Guid InstrumentId, int Id) : IQuery<Bill>;

internal class GetHandler(IQueryable<Domain.Entities.Utility.Account> accounts) : IQueryHandler<Get, Bill>
{
    public async ValueTask<Bill> Handle(Get query, CancellationToken cancellationToken)
    {
        var account = await accounts.Specify(new BillDetailsSpecification()).Where(a => a.Id == query.InstrumentId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Accoutn not found");

        var bill = account.Bills.Where(b => b.Id == query.Id).FirstOrDefault() ?? throw new NotFoundException("Bill not found");

        return bill.ToModel();
    }
}
