using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Asm.MooBank.Services;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateBalance(Guid InstrumentId, CreateTransaction BalanceUpdate) : ICommand<Models.Transaction>;

internal class UpdateBalanceHandler(IInstrumentRepository accountRepository, ITransactionRepository transactionRepository, ISecurity security, IUnitOfWork unitOfWork) :  ICommandHandler<UpdateBalance, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(UpdateBalance command, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(command.InstrumentId);

        var account = await accountRepository.Get(command.InstrumentId, cancellationToken);

        if (account is not Domain.Entities.Instrument.TransactionInstrument transactionAccount)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        var amount = command.BalanceUpdate.Amount - transactionAccount.Balance;

        var transaction = transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = transactionAccount,
            Amount = amount,
            Description = command.BalanceUpdate.Description ?? "Balance adjustment",
            Source = "Web",
            TransactionTime = command.BalanceUpdate.TransactionTime.LocalDateTime,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
