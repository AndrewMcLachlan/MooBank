using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Instruments.Commands.Recurring;

[DisplayName("CreateRecurringTransaction")]
public record Create(Guid InstrumentId, Guid VirtualInstrumentId, [FromBody] RecurringTransactionDetails RecurringTransaction) : ICommand<Models.Recurring.RecurringTransaction>;

internal class CreateHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Create command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, new VirtualInstrumentSpecification(), cancellationToken);

        var virtualInstrument = instrument.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualInstrumentId) ?? throw new NotFoundException();

        var details = command.RecurringTransaction;

        var recurringTransaction = virtualInstrument.AddRecurringTransaction(details.Description, details.Amount, details.Schedule, details.NextRun);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
