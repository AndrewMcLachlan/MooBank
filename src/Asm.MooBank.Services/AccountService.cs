using Asm.Domain;
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
    private readonly Domain.Entities.Account.IInstitutionAccountRepository _accountRepository;
    private readonly IAccountHolderRepository _accountHolderRepository;
    private readonly ISecurityRepository _securityRepository;
    private readonly IUserDataProvider _userDataProvider;

    public AccountService(IUnitOfWork unitOfWork, IRunRulesQueue queue, Domain.Entities.Account.IInstitutionAccountRepository accountRepository, IAccountHolderRepository accountHolderRepository, IUserDataProvider userDataProvider, ISecurityRepository securityRepository) : base(unitOfWork)
    {
        _queue = queue;
        _accountRepository = accountRepository;
        _accountHolderRepository = accountHolderRepository;
        _securityRepository = securityRepository;
        _userDataProvider = userDataProvider;
    }

    public async Task<InstitutionAccount> Create(InstitutionAccount account)
    {
        var entity = (Domain.Entities.Account.InstitutionAccount)account;

        entity.AccountHolders.Add(await _accountHolderRepository.GetCurrent());

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
        entity.IncludeInPosition = account.IncludeInPosition;
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

    public async Task<InstitutionAccount> GetAccount(Guid id) => await GetAccountEntity(id);

    public async Task<Account> SetBalance(Guid id, decimal balance)
    {
        // TODO: Create transaction when modifying balance
        var account = await GetAccountEntity(id);

        if (account.AccountController != AccountController.Manual) throw new InvalidOperationException("Cannot manually adjust balance of non-manually controlled account");

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

        return new AccountsList
        {

            PositionedAccounts = accounts.Where(a => a.IncludeInPosition).Select(a => (InstitutionAccount)a),
            OtherAccounts = accounts.Where(a => !a.IncludeInPosition).Select(a => (InstitutionAccount)a),
            Position = position,
        };
    }

    public void RunTransactionRules(Guid accountId)
    {
        _queue.QueueRunRules(accountId);
    }

    private async Task<Domain.Entities.Account.InstitutionAccount> GetAccountEntity(Guid id)
    {
        var entity = await _accountRepository.Get(id);

        _securityRepository.AssertPermission(entity);

        return entity;
    }
}
