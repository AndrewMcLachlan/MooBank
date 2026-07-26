using Asm.MooBank.Modules.Instruments.Models.Recurring;

namespace Asm.MooBank.Modules.Instruments.Queries.Recurring;

public record GetAll(Guid InstrumentId, Guid VirtualInstrumentId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Instrument.Instrument> instruments) : IQueryHandler<GetAll, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var instrument = (await instruments.Include(a => a.VirtualInstruments.Where(v => v.Id == query.VirtualInstrumentId)).ThenInclude(v => v.RecurringTransactions).SingleOrDefaultAsync(a => a.Id == query.InstrumentId, cancellationToken))
            ?? throw new NotFoundException($"Instrument with ID {query.InstrumentId} was not found.");

        var virtualInstrument = instrument.VirtualInstruments.SingleOrDefault(v => v.Id == query.VirtualInstrumentId)
            ?? throw new NotFoundException($"Virtual instrument with ID {query.VirtualInstrumentId} for instrument ID {query.InstrumentId} was not found.");

        return virtualInstrument.RecurringTransactions.ToModel();
    }
}
