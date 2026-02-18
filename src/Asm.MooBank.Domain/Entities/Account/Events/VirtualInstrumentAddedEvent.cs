namespace Asm.MooBank.Domain.Entities.Account.Events;

public record VirtualInstrumentAddedEvent(VirtualInstrument Instrument, decimal OpeningBalance) : IDomainEvent;

