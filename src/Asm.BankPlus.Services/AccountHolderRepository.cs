﻿using System.Linq;
using System.Threading.Tasks;
using Asm.BankPlus.Data;
using Asm.BankPlus.Models;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Security;
using Asm.Security;
using Microsoft.EntityFrameworkCore;

namespace Asm.BankPlus.Services
{
    public class AccountHolderRepository : DataRepository, IAccountHolderRepository
    {
        private readonly IUserDataProvider _userDataProvider;


        public AccountHolderRepository(BankPlusContext dataContext, IUserDataProvider userDataProvider) : base(dataContext)
        {
            _userDataProvider = userDataProvider;
        }

        public async Task<AccountHolder> GetCurrent()
        {
            var accountHolder = await DataContext.AccountHolders.Where(a => a.AccountHolderId == _userDataProvider.CurrentUserId).SingleOrDefaultAsync();

            if (accountHolder != null)
            {
                return (AccountHolder)accountHolder;
            }

            accountHolder = await _userDataProvider.GetCurrentUser();

            DataContext.Add(accountHolder);

            await DataContext.SaveChangesAsync();

            return (AccountHolder)accountHolder;
        }
    }
}