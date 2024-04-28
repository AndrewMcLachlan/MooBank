using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions.Events;

namespace Asm.MooBank.Domain.Entities.Account.EventHandlers;
internal class TransactionAddedHandler(IInstrumentRepository instrumentRepository) : IDomainEventHandler<TransactionAddedEvent>
{
    public async Task Handle(TransactionAddedEvent request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(request.Transaction.AccountId, cancellationToken);

        if (instrument.Controller == Controller.Import)
        {
            return;
        }

        instrument.Balance += request.Transaction.Amount;
    }
}
