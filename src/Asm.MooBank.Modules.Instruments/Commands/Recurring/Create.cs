using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Instruments.Commands.Recurring;

public record Create(Guid AccountId, Guid VirtualAccountId, string? Description, decimal Amount, ScheduleFrequency Schedule, DateOnly NextRun) : InstrumentIdCommand(AccountId), ICommand<Models.Recurring.RecurringTransaction>
{
    public static ValueTask<Create> BindAsync(HttpContext context) => BindHelper.BindWithInstrumentIdAsync<Create>(context);
}

internal class CreateHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Create, Models.Recurring.RecurringTransaction>
{
    public async ValueTask<Models.Recurring.RecurringTransaction> Handle(Create command, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(command.InstrumentId);

        var account = await accountRepository.Get(command.InstrumentId, new RecurringTransactionSpecification(), cancellationToken);

        var virtualAccount = account.VirtualInstruments.SingleOrDefault(v => v.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        var recurringTransaction = new Domain.Entities.Account.RecurringTransaction
        {
            VirtualAccountId = command.VirtualAccountId,
            Amount = command.Amount,
            Description = command.Description,
            Schedule = command.Schedule,
            NextRun = command.NextRun,
        };

        virtualAccount.RecurringTransactions.Add(recurringTransaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return recurringTransaction.ToModel();
    }
}
