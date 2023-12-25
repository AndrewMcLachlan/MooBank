using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Transactions.Commands.Transactions;

public record Create(Guid AccountId, decimal Amount, string Description, string? Reference, DateTimeOffset TransactionTime) : AccountIdCommand(AccountId), ICommand<Models.Transaction>
{
    public static ValueTask<Create> BindAsync(HttpContext httpContext) => BindHelper.BindWithAccountIdAsync<Create>(httpContext);
}

internal class CreateHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(Create command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var accountId, out var amount, out var description, out var reference, out var transactionTime);

        Security.AssertAccountPermission(accountId);

        TransactionType transactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit;

        Transaction transaction = new()
        {
            Amount = amount,
            AccountId = accountId,
            Description = description,
            Reference = reference,
            Source = "Web",
            TransactionTime = transactionTime.LocalDateTime,
            TransactionType = transactionType,
        };

        transactionRepository.Add(transaction);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
