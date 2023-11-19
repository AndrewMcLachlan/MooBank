using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record Add(decimal Amount, Guid AccountId, bool IsRecurring, string? Description = null) : ICommand<Models.Transaction>;

internal class AddHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Add, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(Add command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var amount, out var accountId, out var isRecurring, out var description);

        Security.AssertAccountPermission(accountId);

        TransactionType transactionType = amount < 0 ?
                                   isRecurring ? TransactionType.RecurringDebit : TransactionType.Debit :
                                   isRecurring ? TransactionType.RecurringCredit : TransactionType.Credit;

        Domain.Entities.Transactions.Transaction transaction = new()
        {
            Amount = amount,
            AccountId = accountId,
            Description = description,
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = transactionType,
        };

        transactionRepository.Add(transaction);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
