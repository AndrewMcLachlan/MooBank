using System.ComponentModel;
using Asm.MooBank.Audit;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;

namespace Asm.MooBank.Modules.Transactions.Commands;

[DisplayName("DeleteTransaction")]
public sealed record Delete(Guid InstrumentId, Guid Id) : InstrumentIdCommand(InstrumentId), ICommand;

internal class DeleteHandler(IInstrumentRepository instrumentRepository, ITransactionRepository transactionRepository, IAuditingUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(command.InstrumentId, cancellationToken);

        if (instrument.Controller is not (Controller.Manual or Controller.Virtual))
        {
            throw new InvalidOperationException("Transactions can only be deleted from manual and virtual accounts.");
        }

        var transaction = await transactionRepository.Get(command.Id, new IncludeSplitsAndOffsetsSpecification(), cancellationToken);

        if (transaction.AccountId != command.InstrumentId)
        {
            throw new NotFoundException("Transaction not found");
        }

        transaction.Delete();

        transactionRepository.Delete(transaction);

        await unitOfWork.SaveChangesAsync("Deleted", "Transaction", transaction.Id, cancellationToken);
    }
}
