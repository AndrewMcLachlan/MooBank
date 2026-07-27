using Asm.MooBank.Modules.Instruments.Models.Recurring;

namespace Asm.MooBank.Modules.Instruments.Queries.Recurring;

public record Get(Guid InstrumentId, Guid VirtualInstrumentId, Guid RecurringTransactionId) : IQuery<RecurringTransaction>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> instruments) : IQueryHandler<Get, RecurringTransaction>
{

    public async ValueTask<RecurringTransaction> Handle(Get query, CancellationToken cancellationToken)
    {
        var recurringTransaction = await instruments.Where(a => a.Id == query.InstrumentId)
            .SelectMany(a => a.VirtualInstruments)
            .Where(v => v.Id == query.VirtualInstrumentId)
            .SelectMany(v => v.RecurringTransactions)
            .SingleOrDefaultAsync(r => r.Id == query.RecurringTransactionId, cancellationToken);

        return recurringTransaction?.ToModel() ?? throw new NotFoundException();
    }
}
