using Asm.MooBank.Domain.Entities.Instrument.Events;

namespace Asm.MooBank.Domain.Entities.Instrument.EventHandlers;

internal class InstrumentChangedEventHandler : IDomainEventHandler<InstrumentCreatedEvent>, IDomainEventHandler<InstrumentUpdatedEvent>
{
    public ValueTask Handle(InstrumentCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        domainEvent.Instrument.LastUpdated = DateTimeOffset.UtcNow;
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(InstrumentUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        domainEvent.Instrument.LastUpdated = DateTimeOffset.UtcNow;
        return ValueTask.CompletedTask;
    }
}
