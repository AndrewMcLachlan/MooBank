using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Accounts.Commands.Recurring;

public record Create(Guid AccountId, Guid VirtualAccountId, string? Description, decimal Amount, ScheduleFrequency Schedule, DateOnly NextRun) : InstrumentIdCommand(AccountId), ICommand<Models.Recurring.RecurringTransaction>
{
    public static ValueTask<Create> BindAsync(HttpContext context) => BindHelper.BindWithInstrumentIdAsync<Create>(context);
}

internal class CreateHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Create command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.Get(command.InstrumentId, new RecurringTransactionSpecification(), cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        var recurringTransaction = virtualAccount.AddRecurringTransaction(command.Description, command.Amount, command.Schedule, command.NextRun);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
