using Asm.Hosting;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Commands.Import;

public record Reprocess(Guid InstrumentId, Guid AccountId) : ICommand;

internal class ReprocessHandler(IBackgroundWorkQueue<ReprocessWorkItem> reprocessTransactionsQueue) : ICommandHandler<Reprocess>
{
    public ValueTask Handle(Reprocess request, CancellationToken cancellationToken)
    {
        reprocessTransactionsQueue.Queue(new ReprocessWorkItem(request.InstrumentId, request.AccountId));
        return ValueTask.CompletedTask;
    }
}
