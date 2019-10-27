using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asm.BankPlus.Data;
using Asm.BankPlus.Data.Entities;
using Asm.BankPlus.Repository;
using Asm.BankPlus.Security;

namespace Asm.BankPlus.Services
{
    public class SecurityRepository : DataRepository, ISecurityRepository
    {
        private readonly IUserDataProvider _userDataProvider;

        public SecurityRepository(BankPlusContext dataContext, IUserDataProvider userDataProvider) : base(dataContext)
        {
            _userDataProvider = userDataProvider;
        }

        public void AssertPermission(Guid accountId)
        {
            if (!DataContext.Accounts.Any(a => a.AccountId == accountId && a.AccountHolderLinks.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))
            {
                throw new NotAuthorisedException("Not authorised to view this account");
            }
        }

        public void AssertPermission(Account account)
        {
            if (!account.AccountHolderLinks.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))
            {
                throw new NotAuthorisedException("Not authorised to view this account");
            }
        }
    }
}
