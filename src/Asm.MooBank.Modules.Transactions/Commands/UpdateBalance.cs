using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateBalance(Guid InstrumentId, CreateTransaction BalanceUpdate) : ICommand<MooBank.Models.Transaction>;

internal class UpdateBalanceHandler(IInstrumentRepository accountRepository, ITransactionRepository transactionRepository, IUserIdProvider userIdProvider, IUnitOfWork unitOfWork) :  ICommandHandler<UpdateBalance, MooBank.Models.Transaction>
{
    public async ValueTask<MooBank.Models.Transaction> Handle(UpdateBalance command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.Get(command.InstrumentId, cancellationToken);

        if (account is not Domain.Entities.Instrument.TransactionInstrument transactionAccount)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        var amount = command.BalanceUpdate.Amount - transactionAccount.Balance;

        var transaction = Domain.Entities.Transactions.Transaction.Create(
            transactionAccount,
            userIdProvider.CurrentUserId,
            amount,
            command.BalanceUpdate.Description ?? "Balance adjustment",
            command.BalanceUpdate.TransactionTime.LocalDateTime,
            TransactionSubType.BalanceAdjustment,
            "Web"
        );

        transactionRepository.Add(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
