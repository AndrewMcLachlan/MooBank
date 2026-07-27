using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Instruments.Commands.Recurring;

[DisplayName("UpdateRecurringTransaction")]
public record Update(Guid InstrumentId, Guid VirtualInstrumentId, Guid RecurringTransactionId, [FromBody] RecurringTransactionDetails RecurringTransaction) : ICommand<Models.Recurring.RecurringTransaction>;

internal class UpdateHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<Update, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Update command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, new VirtualInstrumentSpecification(), cancellationToken);

        var virtualInstrument = instrument.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualInstrumentId) ?? throw new NotFoundException();

        var recurringTransaction = virtualInstrument.RecurringTransactions.SingleOrDefault(rt => rt.Id == command.RecurringTransactionId) ?? throw new NotFoundException();

        var details = command.RecurringTransaction;

        recurringTransaction.Amount = details.Amount;
        recurringTransaction.Description = details.Description;
        recurringTransaction.Schedule = details.Schedule;
        recurringTransaction.NextRun = details.NextRun;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
