using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record Run(Guid InstrumentId) : ICommand;

internal class RunHandler(IRunRulesQueue runRulesQueue) : ICommandHandler<Run>
{
    public ValueTask Handle(Run request, CancellationToken cancellationToken)
    {
        runRulesQueue.QueueRunRules(request.InstrumentId);

        return ValueTask.CompletedTask;
    }
}
