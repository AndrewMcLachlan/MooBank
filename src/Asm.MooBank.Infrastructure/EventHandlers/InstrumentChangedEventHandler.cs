using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;
using LazyCache;

namespace Asm.MooBank.Infrastructure.EventHandlers;

internal class InstrumentChangedEventHandler(IAppCache cache, User user) : IDomainEventHandler<InstrumentCreatedEvent>, IDomainEventHandler<InstrumentUpdatedEvent>
{
    public ValueTask Handle(InstrumentCreatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        ClearCache();

    public ValueTask Handle(InstrumentUpdatedEvent domainEvent, CancellationToken cancellationToken = default) =>
        ClearCache();

    private ValueTask ClearCache()
    {
        cache.Remove(CacheKeys.UserCacheKey(user.Id));
        return ValueTask.CompletedTask;
    }
}
