namespace Asm.MooBank.Domain.Entities.Account.Events;
internal record AccountAddedEvent(LogicalAccount Account, decimal OpeningBalance, DateOnly OpenedDate) : IDomainEvent;

