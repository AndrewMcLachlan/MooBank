using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
{
    public class AccountRepository : DataRepository, IAccountRepository
    {
        public AccountRepository(BankPlusContext dataContext) : base(dataContext)
        {
        }

        public async Task<Account> GetAccount(Guid id)
        {
            return await GetAccountEntity(id);
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            //TODO: Filtering based on user
            return (await DataContext.Accounts.ToListAsync()).Select(a => (Account)a);
        }

        public async Task<Account> SetBalance(Guid id, decimal balance)
        {
            var account = await GetAccountEntity(id);

            account.AccountBalance = balance;

            await DataContext.SaveChangesAsync();

            return account;
        }

        public async Task<Account> SetBalance(Guid id, decimal balance, decimal availableBalance)
        {
            var account = await GetAccountEntity(id);

            account.AccountBalance = balance;
            account.AvailableBalance = availableBalance;

            await DataContext.SaveChangesAsync();

            return account;
        }

        private async Task<Data.Entities.Account> GetAccountEntity(Guid id)
        {
            return await DataContext.Accounts.Where(a => a.AccountId == id).SingleOrDefaultAsync();
        }
    }
}
