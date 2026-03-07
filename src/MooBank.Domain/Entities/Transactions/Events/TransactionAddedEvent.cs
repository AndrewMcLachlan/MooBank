namespace Asm.MooBank.Domain.Entities.Transactions.Events;

internal record TransactionAddedEvent(Transaction Transaction) : IDomainEvent;

