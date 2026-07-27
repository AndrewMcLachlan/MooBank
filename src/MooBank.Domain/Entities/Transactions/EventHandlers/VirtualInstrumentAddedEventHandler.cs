using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Security;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;

internal class VirtualInstrumentAddedEventHandler(IUserIdProvider userIdProvider, ITransactionRepository transactionRepository) : IDomainEventHandler<VirtualInstrumentAddedEvent>
{
    public ValueTask Handle(VirtualInstrumentAddedEvent request, CancellationToken cancellationToken)
    {
        if (request.OpeningBalance == 0) return ValueTask.CompletedTask;
        transactionRepository.Add(Transaction.Create(
            request.Instrument,
            userIdProvider.CurrentUserId,
            request.OpeningBalance,
            "Opening Balance",
            DateTime.UtcNow.Date, // TODO: Local date time
            TransactionSubType.OpeningBalance,
            "Event"
        ));

        return ValueTask.CompletedTask;
    }
}
