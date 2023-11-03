using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record Run(Guid AccountId) : ICommand;

internal class RunHandler(IRunRulesQueue runRulesQueue, ISecurity security) : ICommandHandler<Run>
{
    private readonly IRunRulesQueue _runRulesQueue = runRulesQueue;
    private readonly ISecurity _security = security;

    public ValueTask Handle(Run request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        _runRulesQueue.QueueRunRules(request.AccountId);

        return ValueTask.CompletedTask;
    }
}
