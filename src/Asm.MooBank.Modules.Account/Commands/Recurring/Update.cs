using System.Net.Http;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Accounts.Commands.Recurring;

public record Update(Guid AccountId, Guid VirtualAccountId, Guid RecurringTransactionId, string? Description, decimal Amount, ScheduleFrequency Schedule, DateOnly NextRun) : AccountIdCommand(AccountId), ICommand<Models.Recurring.RecurringTransaction>
{
    public static async ValueTask<Update> BindAsync(HttpContext httpContext)
    {
        Update update = await BindHelper.BindWithAccountIdAsync<Update>(httpContext);

        if (!Guid.TryParse(httpContext.Request.RouteValues["recurringTransactionId"] as string, out Guid recurringTransactionId)) throw new BadHttpRequestException("Invalid recurring transaction ID");

        return update with { RecurringTransactionId = recurringTransactionId };
    }
}

internal class UpdateHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Update command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);

        var account = await accountRepository.Get(command.AccountId, new RecurringTransactionSpecification(), cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        var recurringTransaction = virtualAccount.RecurringTransactions.SingleOrDefault(rt => rt.Id == command.RecurringTransactionId) ?? throw new NotFoundException();

        recurringTransaction.Amount = command.Amount;
        recurringTransaction.Description = command.Description;
        recurringTransaction.Schedule = command.Schedule;
        recurringTransaction.NextRun = command.NextRun;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
