using Asm.MooBank.Domain.Entities.Account.Events;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;

internal class VirtualInstrumentAddedEventHandler(Models.User user, ITransactionRepository transactionRepository) : IDomainEventHandler<VirtualInstrumentAddedEvent>
{
    public ValueTask Handle(VirtualInstrumentAddedEvent request, CancellationToken cancellationToken)
    {
        if (request.OpeningBalance == 0) return ValueTask.CompletedTask;
        transactionRepository.Add(Transaction.Create(
            request.Instrument,
            user.Id,
            request.OpeningBalance,
            "Opening Balance",
            DateTime.UtcNow.Date, // TODO: Local date time
            TransactionSubType.OpeningBalance,
            "Event"
        ));

        return ValueTask.CompletedTask;
    }
}
