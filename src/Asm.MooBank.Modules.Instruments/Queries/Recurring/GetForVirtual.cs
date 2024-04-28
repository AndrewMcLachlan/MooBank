using Asm.MooBank.Modules.Instruments.Models.Recurring;

namespace Asm.MooBank.Modules.Instruments.Queries.Recurring;

public record GetForVirtual(Guid AccountId, Guid VirtualAccountId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetForVirtualHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts, ISecurity security) : IQueryHandler<GetForVirtual, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetForVirtual query, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(query.AccountId);

        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.Id == query.VirtualAccountId) ?? throw new NotFoundException();

        return virtualAccount.RecurringTransactions.ToModel();
    }
}
