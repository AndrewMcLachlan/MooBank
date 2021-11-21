using Asm.BankPlus.Data.Entities;
using Asm.BankPlus.Security;
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
                return accountHolder;
            }

            accountHolder = await _userDataProvider.GetCurrentUser();

            DataContext.Add(accountHolder);

            await DataContext.SaveChangesAsync();

            return accountHolder;
        }
    }
}
