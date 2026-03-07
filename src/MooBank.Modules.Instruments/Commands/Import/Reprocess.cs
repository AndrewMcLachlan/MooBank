using Asm.MooBank.Queues;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Reprocess(Guid InstrumentId, Guid AccountId) : ICommand;

internal class ReprocessHandler(IReprocessTransactionsQueue reprocessTransactionsQueue) : ICommandHandler<Reprocess>
{
    public ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        reprocessTransactionsQueue.QueueReprocessTransactions(request.InstrumentId, request.AccountId);
        return ValueTask.CompletedTask;
    }
}
