using Asm.Domain;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;
using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Security;

namespace Asm.MooBank.Services;

public interface IAccountService
{
    Task<IEnumerable<InstitutionAccount>> GetAccounts(CancellationToken cancellationToken = default);

    Task<InstitutionAccount> GetAccount(Guid id);

    Task<Account> SetBalance(Guid id, decimal balance);

    Task<InstitutionAccount> Create(InstitutionAccount account);

    Task<InstitutionAccount> Update(InstitutionAccount account);

    Task<decimal> GetPosition();

    Task<AccountsList> GetFormattedAccounts(CancellationToken cancellation = default);

    void RunTransactionRules(Guid accountId);
}

public class AccountService : ServiceBase, IAccountService
{
    private readonly IRunRulesQueue _queue;
    private readonly IInstitutionAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly Domain.Repositories.ISecurity _securityRepository;
    private readonly IUserDataProvider _userDataProvider;

    public AccountService(IUnitOfWork unitOfWork, IRunRulesQueue queue, IInstitutionAccountRepository accountRepository, IAccountHolderRepository accountHolderRepository, ITransactionRepository transactionRepository, IUserDataProvider userDataProvider, Domain.Repositories.ISecurity securityRepository) : base(unitOfWork)
    {
        _queue = queue;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _accountHolderRepository = accountHolderRepository;
        _securityRepository = securityRepository;
        _userDataProvider = userDataProvider;
    }

    public async Task<InstitutionAccount> Create(InstitutionAccount account)
    {
        var entity = (Domain.Entities.Account.InstitutionAccount)account;

        entity.SetAccountHolder(_userDataProvider.CurrentUserId);

        //entity.AccountHolders.Add(await _accountHolderRepository.GetCurrent());

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

    public async Task<InstitutionAccount> Update(InstitutionAccount account)
    {
        var entity = await GetAccountEntity(account.Id);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.SetAccountGroup(account.AccountGroupId, _userDataProvider.CurrentUserId);
        entity.AccountType = account.AccountType;

        if (account.Controller != entity.AccountController)
        {
            entity.AccountController = account.Controller;
            if (account.Controller == AccountController.Import)
            {
                var importerType = await _accountRepository.GetImporterType(account.ImporterTypeId ?? throw new InvalidOperationException("Import account without importer type"));

                if (importerType == null) throw new NotFoundException("Unknown importer type ID " + account.ImporterTypeId);

                entity.ImportAccount = new Domain.Entities.Account.ImportAccount
                {
                    AccountId = entity.AccountId,
                    ImporterTypeId = account.ImporterTypeId!.Value,
                };
            }
            else if (entity.ImportAccount != null)
            {
                _accountRepository.RemoveImportAccount(entity.ImportAccount);
            }
        }

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public Task<InstitutionAccount> GetAccount(Guid id) => GetAccountEntity(id).ToModelAsync(_userDataProvider.CurrentUserId);

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
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        account.Balance = balance;

        await UnitOfWork.SaveChangesAsync();

        return account;
    }

    public Task<decimal> GetPosition() => _accountRepository.GetPosition();

    public async Task<IEnumerable<InstitutionAccount>> GetAccounts(CancellationToken cancellationToken = default) => (await _accountRepository.GetAll(cancellationToken)).Select(a => (InstitutionAccount)a);


    public async Task<AccountsList> GetFormattedAccounts(CancellationToken cancellation = default)
    {
        var accounts = await _accountRepository.GetAll(cancellation);
        var position = await _accountRepository.GetPosition();
        var userId = _userDataProvider.CurrentUserId;

        var groups = accounts.Select(a => a.GetAccountGroup(userId)).Where(a => a != null).Distinct().Select(ag => new AccountListGroup
        {
            Name = ag.Name,
            Accounts = accounts.Where(a => a.GetAccountGroup(userId)?.Id == ag.Id).ToModel(),
            Position = ag.ShowPosition ? accounts.Where(a => a.GetAccountGroup(userId)?.Id == ag.Id).Sum(a => a.Balance) : null,
        });

        var otherAccounts = new AccountListGroup[] {
            new AccountListGroup
            {
                Name = "Other Accounts",
                Accounts = accounts.Where(a => a.GetAccountGroup(userId) == null).ToModel(),
            }
        };

        return new AccountsList
        {
            AccountGroups = groups.Union(otherAccounts),
            Position = groups.Sum(g => g.Position ?? 0),
        };
    }

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
