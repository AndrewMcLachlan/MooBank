using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Instruments.Queries.Rules;

namespace Asm.MooBank.Modules.Instruments.Commands.Rule;

public record Update(Guid AccountId, int RuleId, Queries.Rules.Rule Rule) : ICommand<Queries.Rules.Rule>;

internal sealed class UpdateRuleHandler(IRuleRepository ruleRepository, ISecurity securityRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Queries.Rules.Rule>
{
    public async ValueTask<Queries.Rules.Rule> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await ruleRepository.Get(request.Rule.Id, cancellationToken);

        securityRepository.AssertInstrumentPermission(entity.InstrumentId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
