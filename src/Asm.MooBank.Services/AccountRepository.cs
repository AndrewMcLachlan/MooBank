﻿using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services
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

            entity.AccountHolders.Add(await _accountHolderRepository.GetCurrent());

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

        public async Task<Account> Update(Account account)
        {
            var entity = await GetAccountEntity(account.Id);

            entity.Name = account.Name;
            entity.Description = account.Description;
            entity.IncludeInPosition = account.IncludeInPosition;
            entity.AccountType = account.AccountType;

            if (entity.AccountController != account.Controller)
            {
                entity.AccountController = account.Controller;

                if (account.Controller == AccountController.Import)
                {
                    var importerType = await DataContext.ImporterTypes.Where(i => i.ImporterTypeId == account.ImporterTypeId).SingleOrDefaultAsync();

                    if (importerType == null) throw new NotFoundException("Unknown importer type ID " + account.ImporterTypeId);

                    var importAccountEntity = new Data.Entities.ImportAccount
                    {
                        AccountId = entity.AccountId,
                        ImporterTypeId = account.ImporterTypeId!.Value,
                    };

                    DataContext.Add(importAccountEntity);
                }
                else if (entity.ImportAccount != null)
                {
                    DataContext.Remove(entity.ImportAccount);
                }
            }

            await DataContext.SaveChangesAsync();

            return entity;
        }

        public async Task<Account> GetAccount(Guid id) => await GetAccountEntity(id);

        public async Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken = default) =>
            await DataContext.Accounts.Include(a => a.VirtualAccounts).Include(a => a.AccountHolders).Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)).Select(a => (Account)a).ToListAsync(cancellationToken);

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
            var entity = await DataContext.Accounts.Include(a => a.ImportAccount).Include(a => a.AccountHolders).Where(a => a.AccountId == id).SingleOrDefaultAsync();

            if (entity == null) throw new NotFoundException("Acccount not found");

            _securityRepository.AssertPermission(entity);

            return entity;
        }
    }
}