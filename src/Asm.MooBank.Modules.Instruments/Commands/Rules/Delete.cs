using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;
public record Delete(Guid Instrument, int RuleId) : ICommand;

internal class DeleteHandler(IRuleRepository ruleRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        await ruleRepository.Delete(request.Instrument, request.RuleId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
