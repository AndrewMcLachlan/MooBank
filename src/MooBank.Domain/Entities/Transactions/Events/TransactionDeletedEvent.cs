namespace Asm.MooBank.Domain.Entities.Transactions.Events;

internal record TransactionDeletedEvent(Guid TransactionId) : IDomainEvent;
