using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record Delete(Guid InstrumentId, Guid VirtualInstrumentId) : ICommand;

internal class DeleteHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, new VirtualAccountSpecification(), cancellationToken);

        if (!instrument.VirtualInstruments.Any(a => a.Id == command.VirtualInstrumentId)) throw new NotFoundException();

        instrumentRepository.Delete(command.VirtualInstrumentId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
