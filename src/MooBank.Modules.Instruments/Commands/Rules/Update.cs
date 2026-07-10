using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Instruments.Models.Rules;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

[DisplayName("UpdateRule")]
public record Update(Guid InstrumentId, int RuleId, UpdateRule Rule) : ICommand<Models.Rules.Rule>;

internal sealed class UpdateRuleHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Models.Rules.Rule>
{
    public async ValueTask<Models.Rules.Rule> Handle(Update command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, cancellationToken);

        var entity = instrument.UpdateRule(command.RuleId, command.Rule.Contains, command.Rule.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
