using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Instruments.Models.Rules;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record Update(Guid InstrumentId, int RuleId, UpdateRule Rule) : ICommand<Models.Rules.Rule>;

internal sealed class UpdateRuleHandler(IRuleRepository ruleRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Models.Rules.Rule>
{
    public async ValueTask<Models.Rules.Rule> Handle(Update command, CancellationToken cancellationToken)
    {
        var entity = await ruleRepository.Get(command.InstrumentId, command.RuleId, cancellationToken);

        entity.Contains = command.Rule.Contains;
        entity.Description = command.Rule.Description;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
