using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Accounts.Commands.Rule;
public record Delete(Guid AccountId, int RuleId) : ICommand;

internal class DeleteHandler(IRuleRepository ruleRepository, ISecurity security, IUnitOfWork unitOfWork) :  ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        await ruleRepository.Delete(request.AccountId, request.RuleId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
