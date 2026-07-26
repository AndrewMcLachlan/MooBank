namespace Asm.MooBank.Domain.Entities.Instrument.Events;

public record VirtualInstrumentAddedEvent(VirtualInstrument Instrument, decimal OpeningBalance) : IDomainEvent;

