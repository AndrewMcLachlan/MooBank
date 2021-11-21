using Asm.BankPlus.Models;
using Asm.BankPlus.Security;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
{
    public class VirtualAccountRepository : DataRepository, IVirtualAccountRepository
    {
        private readonly IAccountHolderRepository _accountHolderRepository;
        private readonly ISecurityRepository _securityRepository;
        private readonly IUserDataProvider _userDataProvider;

        public VirtualAccountRepository(BankPlusContext dataContext, IAccountHolderRepository accountHolderRepository, IUserDataProvider userDataProvider, ISecurityRepository securityRepository) : base(dataContext)
        {
            _accountHolderRepository = accountHolderRepository;
            _securityRepository = securityRepository;
            _userDataProvider = userDataProvider;
        }

        public async Task<VirtualAccount> Create(Guid accountId, VirtualAccount account)
        {
            _securityRepository.AssertPermission(accountId);

            var entity = (Data.Entities.VirtualAccount)account;
            entity.AccountId = accountId;

            entity = DataContext.Add(entity).Entity;

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task Delete(Guid accountId, Guid virtualAccountId)
        {
            _securityRepository.AssertPermission(accountId);

            var entity = await GetEntity(accountId, virtualAccountId);

            DataContext.Remove(entity);

            await DataContext.SaveChangesAsync();
        }

        public async Task<VirtualAccount> Get(Guid accountId, Guid virtualAccountId)
        {
            return await GetEntity(accountId, virtualAccountId);
        }

        public async Task<VirtualAccount> Update(Guid accountId, VirtualAccount account)
        {
            _securityRepository.AssertPermission(accountId);

            var entity = await GetEntity(accountId, account.Id);

            entity.Name = account.Name;
            entity.Description = account.Description;
            entity.Balance = account.Balance;

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<VirtualAccount> UpdateBalance(Guid accountId, Guid virtualAccountId, decimal balance)
        {
            _securityRepository.AssertPermission(accountId);

            var entity = await GetEntity(accountId, virtualAccountId);

            entity.Balance = balance;

            await DataContext.SaveChangesAsync();

            return entity;
        }

        private async Task<Data.Entities.VirtualAccount> GetEntity(Guid accountId, Guid virtualAccountId)
        {
            _securityRepository.AssertPermission(accountId);

            var entity = await DataContext.VirtualAccounts.SingleOrDefaultAsync(v => v.AccountId == accountId && v.VirtualAccountId == virtualAccountId);

            if (entity == null) throw new NotFoundException($"Virtual account with ID {virtualAccountId} not found on account {accountId}");

            return entity;
        }
    }
}
