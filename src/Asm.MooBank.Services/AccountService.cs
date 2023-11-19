using Asm.Domain;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models;

namespace Asm.MooBank.Services;

public interface IAccountService
{
    Task<Account> SetBalance(Guid id, decimal balance);

    Task<InstitutionAccount> Create(InstitutionAccount account);

    Task<decimal> GetPosition();

    void RunTransactionRules(Guid accountId);
}

public class AccountService : ServiceBase, IAccountService
{
    private readonly IRunRulesQueue _queue;
    private readonly IInstitutionAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISecurity _securityRepository;
    private readonly IUserDataProvider _userDataProvider;

    public AccountService(IUnitOfWork unitOfWork, IRunRulesQueue queue, IInstitutionAccountRepository accountRepository, ITransactionRepository transactionRepository, IUserDataProvider userDataProvider, ISecurity securityRepository) : base(unitOfWork)
    {
        _queue = queue;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _securityRepository = securityRepository;
        _userDataProvider = userDataProvider;
    }

    public async Task<InstitutionAccount> Create(InstitutionAccount account)
    {
        var entity = (Domain.Entities.Account.InstitutionAccount)account;

        entity.SetAccountHolder(_userDataProvider.CurrentUserId);
        entity.SetAccountGroup(account.AccountGroupId, _userDataProvider.CurrentUserId);

        _accountRepository.Add(entity);

        if (account.ImporterTypeId != null)
        {
            entity.ImportAccount = new Domain.Entities.Account.ImportAccount
            {
                ImporterTypeId = account.ImporterTypeId.Value,
            };
        }

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task<Account> SetBalance(Guid id, decimal balance)
    {
        var account = await GetAccountEntity(id);

        if (account.AccountController != AccountController.Manual) throw new InvalidOperationException("Cannot manually adjust balance of non-manually controlled account");

        var amount = account.Balance - balance;

        _transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = account,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        account.Balance = balance;

        await UnitOfWork.SaveChangesAsync();

        return account;
    }

    public Task<decimal> GetPosition() => _accountRepository.GetPosition();

    public void RunTransactionRules(Guid accountId)
    {
        _queue.QueueRunRules(accountId);
    }

    private async Task<Domain.Entities.Account.InstitutionAccount> GetAccountEntity(Guid id)
    {
        var entity = await _accountRepository.Get(id);

        _securityRepository.AssertAccountPermission(entity);

        return entity;
    }
}
