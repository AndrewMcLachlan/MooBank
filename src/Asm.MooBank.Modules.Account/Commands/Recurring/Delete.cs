using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;

namespace Asm.MooBank.Modules.Accounts.Commands.Recurring;

public record Delete(Guid AccountId, Guid RecurringTransactionId) : ICommand;

internal class DeleteHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Delete>
{
    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);

        var account = await accountRepository.Get(command.AccountId, new RecurringTransactionSpecification(), cancellationToken);

        var recurringTransaction = account.VirtualInstruments.SelectMany(v => v.RecurringTransactions).SingleOrDefault(r => r.Id == command.RecurringTransactionId) ?? throw new NotFoundException();

        recurringTransaction.VirtualAccount.RecurringTransactions.Remove(recurringTransaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
