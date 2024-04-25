using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Asm.MooBank.Services;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateBalance(Guid AccountId, CreateTransaction BalanceUpdate) : ICommand<Models.Transaction>;

internal class UpdateBalanceHandler(IInstrumentRepository accountRepository, ITransactionRepository transactionRepository, AccountHolder accountHolder, ICurrencyConverter currencyConverter, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<UpdateBalance, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(UpdateBalance command, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(command.AccountId);

        var account = await accountRepository.Get(command.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.TransactionAccount transactionAccount)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        var amount = command.BalanceUpdate.Amount - transactionAccount.CalculatedBalance;

        var transaction = transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = transactionAccount,
            Amount = amount,
            Description = command.BalanceUpdate.Description ?? "Balance adjustment",
            Source = "Web",
            TransactionTime = command.BalanceUpdate.TransactionTime.LocalDateTime,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
