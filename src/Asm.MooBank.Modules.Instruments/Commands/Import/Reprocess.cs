using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Reprocess(Guid InstrumentId) : ICommand;

internal class ReprocessHandler(IReprocessTransactionsQueue reprocessTransactionsQueue) : ICommandHandler<Reprocess>
{
    public ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        reprocessTransactionsQueue.QueueReprocessTransactions(request.InstrumentId);
        return ValueTask.CompletedTask;
    }
}
