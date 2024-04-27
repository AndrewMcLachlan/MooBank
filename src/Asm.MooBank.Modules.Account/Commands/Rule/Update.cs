using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Modules.Accounts.Models.Rules;

namespace Asm.MooBank.Modules.Accounts.Commands.Rule;

public record Update(Guid AccountId, int RuleId, Models.Rules.Rule Rule) : ICommand<Models.Rules.Rule>;

internal class UpdateRuleHandler(IRuleRepository transactionTagRuleRepository, ISecurity securityRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Models.Rules.Rule>
{
    public async ValueTask<Models.Rules.Rule> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await transactionTagRuleRepository.Get(request.Rule.Id, cancellationToken);

        securityRepository.AssertInstrumentPermission(entity.InstrumentId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
