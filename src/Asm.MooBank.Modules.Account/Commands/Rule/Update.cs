using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record Update(Guid AccountId, int RuleId, Models.Rule Rule) : ICommand<Models.Rule>;

internal class UpdateRuleHandler(IRuleRepository transactionTagRuleRepository, ISecurity securityRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Models.Rule>
{
    public async ValueTask<Models.Rule> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await transactionTagRuleRepository.Get(request.Rule.Id, cancellationToken);

        securityRepository.AssertAccountPermission(entity.AccountId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
