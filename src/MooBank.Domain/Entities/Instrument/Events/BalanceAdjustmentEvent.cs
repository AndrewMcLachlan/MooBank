namespace Asm.MooBank.Domain.Entities.Instrument.Events;

public record BalanceAdjustmentEvent(Instrument Instrument, decimal Amount, string Source) : IDomainEvent;
