using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Domain.Entities.StockHolding;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class StockHoldingRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, StockHolding, Guid>(context), IStockHoldingRepository
{
    public override StockHolding Add(StockHolding entity)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new InstrumentCreatedEvent(tracked));
        return tracked;
    }

    public override StockHolding Update(StockHolding entity)
    {
        var tracked = base.Update(entity);
        tracked.Events.Add(new InstrumentUpdatedEvent(tracked));
        return tracked;
    }
}
