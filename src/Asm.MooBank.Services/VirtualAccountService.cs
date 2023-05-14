using Asm.Domain;
using IVirtualAccountRepository = Asm.MooBank.Domain.Entities.Account.IVirtualAccountRepository;
using Asm.MooBank.Models;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Services;

public interface IVirtualAccountService
{
    Task<VirtualAccount> Create(Guid accountId, VirtualAccount account);

    Task Delete(Guid accountId, Guid virtualAccountId);

    Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId);

    Task<VirtualAccount> Update(Guid accountId, VirtualAccount account);

    Task<VirtualAccount> UpdateBalance(Guid accountId, Guid virtualAccountId, decimal balance);
}

public class VirtualAccountService : ServiceBase, IVirtualAccountService
{
    private readonly ISecurity _securityRepository;
    private readonly IVirtualAccountRepository _virtualAccountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public VirtualAccountService(IUnitOfWork unitOfWork, IVirtualAccountRepository virtualAccountRepository, ITransactionRepository transactionRepository, ISecurity securityRepository) : base(unitOfWork)
    {
        _virtualAccountRepository = virtualAccountRepository;
        _transactionRepository = transactionRepository;
        _securityRepository = securityRepository;
    }

    public async Task<VirtualAccount> Create(Guid accountId, VirtualAccount account)
    {
        _securityRepository.AssertAccountPermission(accountId);

        var entity = (Domain.Entities.Account.VirtualAccount)account;

        entity.AccountId = accountId;

        entity = _virtualAccountRepository.Add(entity);

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task Delete(Guid accountId, Guid virtualAccountId)
    {
        _securityRepository.AssertAccountPermission(accountId);

        var entity = await GetEntity(accountId, virtualAccountId);

        _virtualAccountRepository.Delete(entity);

        await UnitOfWork.SaveChangesAsync();
    }

    public async Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId) => await GetEntity(accountId, virtualAccountId);

    public async Task<VirtualAccount> Update(Guid accountId, VirtualAccount account)
    {
        _securityRepository.AssertAccountPermission(accountId);

        var entity = await GetEntity(accountId, account.Id);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.Balance = account.Balance;

        await UnitOfWork.SaveChangesAsync();

        return entity;
    }

    public async Task<VirtualAccount> UpdateBalance(Guid accountId, Guid virtualAccountId, decimal balance)
    {
        _securityRepository.AssertAccountPermission(accountId);

        var account = await GetEntity(accountId, virtualAccountId);

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

    private async Task<Domain.Entities.Account.VirtualAccount> GetEntity(Guid accountId, Guid virtualAccountId)
    {
        _securityRepository.AssertAccountPermission(accountId);

        var entity = await _virtualAccountRepository.Get(virtualAccountId);

        return entity;
    }
}
