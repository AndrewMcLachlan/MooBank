using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;

namespace Asm.MooBank.Modules.Instruments.Commands.Recurring;

// A DELETE carries no body, so this binds straight from the route parameters rather than going
// through BindHelper (which deserialises the request body) like Create and Update do.
public record Delete(Guid InstrumentId, Guid VirtualInstrumentId, Guid RecurringTransactionId) : ICommand;

internal class DeleteHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, new VirtualInstrumentSpecification(), cancellationToken);

        var virtualInstrument = instrument.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualInstrumentId) ?? throw new NotFoundException();

        // RemoveRecurringTransaction throws NotFoundException when the transaction belongs to a
        // different virtual instrument, so the route's virtualInstrumentId is enforced, not assumed.
        virtualInstrument.RemoveRecurringTransaction(command.RecurringTransactionId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
