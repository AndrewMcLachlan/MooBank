using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;

internal class AccountAddedEventHandler(Models.User user, ITransactionRepository transactionRepository) : IDomainEventHandler<AccountAddedEvent>
{
    public ValueTask Handle(AccountAddedEvent request, CancellationToken cancellationToken)
    {
        if (request.OpeningBalance == 0) return ValueTask.CompletedTask;

        transactionRepository.Add(
        Transaction.Create(
            request.Account,
            user.Id,
            request.OpeningBalance,
            "Opening Balance",
            request.OpenedDate.ToDateTime(TimeOnly.MinValue),
            TransactionSubType.OpeningBalance,
            "Event"));

        return ValueTask.CompletedTask;
    }
}
