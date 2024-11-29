using Asm.MooBank.Modules.Accounts.Models.Recurring;

namespace Asm.MooBank.Modules.Accounts.Queries.Recurring;

public record GetForVirtual(Guid AccountId, Guid VirtualAccountId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetForVirtualHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts) : IQueryHandler<GetForVirtual, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetForVirtual query, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.Id == query.VirtualAccountId) ?? throw new NotFoundException();

        return virtualAccount.RecurringTransactions.ToModel();
    }
}
