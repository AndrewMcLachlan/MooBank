using Asm.MooBank.Domain.Entities.Account.Events;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;
internal class AccountAddedEventHandler(Models.User user) : IDomainEventHandler<AccountAddedEvent>
{
    public Task Handle(AccountAddedEvent request, CancellationToken cancellationToken)
    {
        request.Account.Transactions.Add(new Transaction
        {
            AccountHolderId = user.Id,
            Amount = request.OpeningBalance,
            Description = "Opening Balance",
            TransactionTime = request.OpeningDate,
            TransactionType = request.OpeningBalance < 0 ? TransactionType.Debit : TransactionType.Credit,
            TransactionSubType = TransactionSubType.OpeningBalance,
            Source = "Event",
        });

        return Task.CompletedTask;
    }
}
