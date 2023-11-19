using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.InstitutionAccount;

public record UpdateBalance(Guid AccountId, decimal Balance) : ICommand<Models.Account.InstitutionAccount>;

internal class UpdateBalanceHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<UpdateBalance, Models.Account.InstitutionAccount>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Account.InstitutionAccount> Handle(UpdateBalance request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Not an institution account.");
        }

        var amount = account.Balance - account.Balance;

        //TODO: Should be done via domain event
        _transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = account,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        account.Balance = request.Balance;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return institutionAccount.ToModel();
    }
}
