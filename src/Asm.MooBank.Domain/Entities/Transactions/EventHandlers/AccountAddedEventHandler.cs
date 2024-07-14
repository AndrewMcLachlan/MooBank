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
            TransactionTime = DateTime.Now,
            TransactionType = request.OpeningBalance < 0 ? TransactionType.BalanceAdjustmentDebit : TransactionType.BalanceAdjustmentCredit,
            Source = "Event",
        });

        return Task.CompletedTask;
    }
}
