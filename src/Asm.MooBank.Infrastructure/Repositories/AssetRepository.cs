using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Instrument.Events;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class AssetRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, Asset, Guid>(context), IAssetRepository
{
    public override Asset Add(Asset entity)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new InstrumentCreatedEvent(tracked));
        return tracked;
    }

    public override Asset Update(Asset entity)
    {
        var tracked = base.Update(entity);
        tracked.Events.Add(new InstrumentUpdatedEvent(tracked));
        return tracked;
    }
}
