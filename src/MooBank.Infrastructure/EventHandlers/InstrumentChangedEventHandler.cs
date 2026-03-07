using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Infrastructure.EventHandlers;

internal class InstrumentChangedEventHandler(HybridCache cache, User user) : IDomainEventHandler<InstrumentCreatedEvent>, IDomainEventHandler<InstrumentUpdatedEvent>
{
    public async ValueTask Handle(InstrumentCreatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        await ClearCache(cancellationToken);

    public async ValueTask Handle(InstrumentUpdatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        await ClearCache(cancellationToken);

    private async ValueTask ClearCache(CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.UserCacheKey(user.Id), cancellationToken);
    }
}
