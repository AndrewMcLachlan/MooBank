namespace Asm.MooBank.Domain.Entities.Instrument.Events;

public record InstrumentCreatedEvent(Instrument Instrument) : IDomainEvent;
