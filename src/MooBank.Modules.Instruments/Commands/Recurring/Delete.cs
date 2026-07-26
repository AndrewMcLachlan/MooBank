using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;

namespace Asm.MooBank.Modules.Accounts.Commands.Recurring;

public record Delete(Guid AccountId, Guid RecurringTransactionId) : ICommand;

internal class DeleteHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.Get(command.AccountId, new RecurringTransactionSpecification(), cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.RecurringTransactions.Any(r => r.Id == command.RecurringTransactionId)) ?? throw new NotFoundException();

        virtualAccount.RemoveRecurringTransaction(command.RecurringTransactionId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
