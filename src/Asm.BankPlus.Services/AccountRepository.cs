using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Microsoft.EntityFrameworkCore;
using Asm.Security;
using Asm.BankPlus.Security;

namespace Asm.BankPlus.Services
{
    public class AccountRepository : DataRepository, IAccountRepository
    {
        private readonly IAccountHolderRepository _accountHolderRepository;
        private readonly ISecurityRepository _securityRepository;
        private readonly IUserDataProvider _userDataProvider;

        public AccountRepository(BankPlusContext dataContext, IAccountHolderRepository accountHolderRepository, IUserDataProvider userDataProvider, ISecurityRepository securityRepository) : base(dataContext)
        {
            _accountHolderRepository = accountHolderRepository;
            _securityRepository = securityRepository;
            _userDataProvider = userDataProvider;
        }

        public async Task<Account> Create(Account account)
        {
            var entity = (Data.Entities.Account)account;

            entity.AccountHolders.Add((Data.Entities.AccountHolder) await _accountHolderRepository.GetCurrent());

            DataContext.Add(entity);

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Account> CreateImportAccount(Account account, int importerTypeId)
        {
            var importerType = await DataContext.ImporterTypes.Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync();

            if (importerType == null) throw new NotFoundException("Unknown importer type ID " + importerTypeId);

            var entity = (Data.Entities.Account)account;

            entity.AccountHolders.Add((Data.Entities.AccountHolder)await _accountHolderRepository.GetCurrent());

            DataContext.Accounts.Add(entity);

            await DataContext.SaveChangesAsync();


            var importAccountEntity = new Data.Entities.ImportAccount
            {
                AccountId = entity.AccountId,
                ImporterTypeId = importerTypeId,
            };

            DataContext.Add(importAccountEntity);

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Account> GetAccount(Guid id)
        {
            return await GetAccountEntity(id);
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await DataContext.Accounts.Include(a => a.AccountHolders).Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)).Select(a => (Account)a).ToListAsync();
        }

        public async Task<Account> SetBalance(Guid id, decimal balance)
        {
            var account = await GetAccountEntity(id);

            account.AccountBalance = balance;

            await DataContext.SaveChangesAsync();

            return account;
        }

        public async Task<Account> SetBalance(Guid id, decimal? balance, decimal? availableBalance)
        {
            var account = await GetAccountEntity(id);

            if (account.AccountController != AccountController.Manual) throw new InvalidOperationException("Cannot manually adjust balance of non-manually controlled account");

            account.AccountBalance = balance ?? account.AccountBalance;
            account.AvailableBalance = availableBalance ?? account.AvailableBalance;

            await DataContext.SaveChangesAsync();

            return account;
        }

        public async Task<decimal> GetPosition()
        {
            return await DataContext.Accounts.Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId) && a.IncludeInPosition).SumAsync(a => a.AccountBalance);
        }

        private async Task<Data.Entities.Account> GetAccountEntity(Guid id)
        {
            var entity = await DataContext.Accounts.Include(a => a.AccountHolders).Where(a => a.AccountId == id).SingleOrDefaultAsync();

            if (entity == null) throw new NotFoundException("Acccount not found");

            _securityRepository.AssertPermission(entity);

            return entity;
        }
    }
}
