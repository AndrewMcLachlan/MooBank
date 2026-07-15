using Asm.Hosting;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record Run(Guid InstrumentId) : ICommand;

internal class RunHandler(IBackgroundWorkQueue<Guid> runRulesQueue) : ICommandHandler<Run>
{
    public ValueTask Handle(Run request, CancellationToken cancellationToken)
    {
        runRulesQueue.Queue(request.InstrumentId);

        return ValueTask.CompletedTask;
    }
}
