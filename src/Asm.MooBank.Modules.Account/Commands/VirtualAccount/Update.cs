using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.VirtualAccount;

public record Update(Guid AccountId, Guid VirtualAccountId, string Name, string Description, decimal Balance) : ICommand<Models.Account.VirtualAccount>;

internal class UpdateHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Account.VirtualAccount>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Update request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Cannot update virtual account on non-institution account.");
        }

        var entity = institutionAccount.VirtualAccounts.SingleOrDefault(va => va.AccountId == request.VirtualAccountId) ?? throw new NotFoundException();

        entity.Name = request.Name;
        entity.Description = request.Description;

        var amount = entity.Balance - request.Balance;

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

        entity.Balance = request.Balance;


        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
