using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Account.Commands.Rule;
public record Delete(Guid AccountId, int RuleId) : ICommand;

internal class DeleteHandler(IRuleRepository ruleRepository, MooBank.Models.AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        await ruleRepository.Delete(request.AccountId, request.RuleId, cancellationToken);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
