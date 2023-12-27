using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Recurring;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Account.Commands.Recurring;

public record Create(Guid AccountId, Guid VirtualAccountId, string? Description, decimal Amount, ScheduleFrequency Schedule) : AccountIdCommand(AccountId), ICommand<Models.Recurring.RecurringTransaction>
{
    public static ValueTask<Create> BindAsync(HttpContext context) => BindHelper.BindWithAccountIdAsync<Create>(context);
}

internal class CreateHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Create, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Create command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);

        var account = await accountRepository.Get(command.AccountId, new RecurringTransactionSpecification(), cancellationToken);

        var virtualAccount = account.VirtualAccounts.SingleOrDefault(v => v.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        var recurringTransaction = new Domain.Entities.Account.RecurringTransaction
        {
            VirtualAccountId = command.VirtualAccountId,
            Amount = command.Amount,
            Description = command.Description,
            Schedule = command.Schedule,
        };

        virtualAccount.RecurringTransactions.Add(recurringTransaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
