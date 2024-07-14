namespace Asm.MooBank.Domain.Entities.Account.Events;
internal record AccountAddedEvent(InstitutionAccount Account, decimal OpeningBalance) : IDomainEvent;

