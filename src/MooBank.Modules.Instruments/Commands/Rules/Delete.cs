using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record Delete(Guid InstrumentId, int RuleId) : ICommand;

internal class DeleteHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(request.InstrumentId, cancellationToken);

        instrument.RemoveRule(request.RuleId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
