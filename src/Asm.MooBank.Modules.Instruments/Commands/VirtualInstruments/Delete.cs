using Asm.MooBank.Commands;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record Delete(Guid InstrumentId, Guid VirtualAccountId) : ICommand;

internal class DeleteHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        instrumentRepository.Delete(command.VirtualAccountId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
