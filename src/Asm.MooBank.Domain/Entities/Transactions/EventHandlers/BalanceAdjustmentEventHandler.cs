using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Transactions.EventHandlers;

internal class BalanceAdjustmentEventHandler(ITransactionRepository transactionRepository) : IDomainEventHandler<BalanceAdjustmentEvent>
{
    public ValueTask Handle(BalanceAdjustmentEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent.Instrument is not TransactionInstrument instrument)
        {
            return ValueTask.CompletedTask;
        }

        var transaction = Transaction.Create(
            instrument,
            accountHolderId: null,
            domainEvent.Amount,
            "Balance adjustment",
            DateTime.Now,
            TransactionSubType.BalanceAdjustment,
            domainEvent.Source
        );

        transactionRepository.Add(transaction);

        return ValueTask.CompletedTask;
    }
}
