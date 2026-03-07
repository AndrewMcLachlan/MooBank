namespace Asm.MooBank.Domain.Entities.Instrument.Events;

public record InstrumentUpdatedEvent(Instrument Instrument) : IDomainEvent;
