using Asm.MooBank.Domain.Entities.Account.Events;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;
internal class VirtualInstrumentAddedEventHandler(Models.User user) : IDomainEventHandler<VirtualInstrumentAddedEvent>
{
    public Task Handle(VirtualInstrumentAddedEvent request, CancellationToken cancellationToken)
    {
        if (request.OpeningBalance == 0) return Task.CompletedTask;

        request.Instrument.Transactions.Add(new Transaction
        {
            AccountHolderId = user.Id,
            Amount = request.OpeningBalance,
            Description = "Opening Balance",
            TransactionTime = DateTime.Now.Date, // TODO: Local date time
            TransactionType = request.OpeningBalance < 0 ? TransactionType.Debit : TransactionType.Credit,
            TransactionSubType = TransactionSubType.OpeningBalance,
            Source = "Event",
        });

        return Task.CompletedTask;
    }
}
