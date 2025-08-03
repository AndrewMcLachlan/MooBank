using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record Create(Guid InstrumentId, decimal Amount, string Description, string? Reference, DateTimeOffset TransactionTime) : InstrumentIdCommand(InstrumentId), ICommand<MooBank.Models.Transaction>
{
    public static ValueTask<Create> BindAsync(HttpContext httpContext) => BindHelper.BindWithInstrumentIdAsync<Create>(httpContext);
}

internal class CreateHandler(IInstrumentRepository accountRepository, ITransactionRepository transactionRepository, IUserIdProvider userIdProvider, IUnitOfWork unitOfWork) :  ICommandHandler<Create, MooBank.Models.Transaction>
{
    public async ValueTask<MooBank.Models.Transaction> Handle(Create command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var instrumentId, out var amount, out var description, out var reference, out var transactionTime);

        var account = await accountRepository.Get(instrumentId, cancellationToken);
        if (account is not Domain.Entities.Instrument.TransactionInstrument transactionAccount)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        Transaction transaction = Transaction.Create(
            transactionAccount,
            userIdProvider.CurrentUserId,
            amount,
            description,
            transactionTime.LocalDateTime,
            null, // TransactionSubType is not set in this command
            "Web"
        );

        transaction.Reference = reference;

        transactionRepository.Add(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
