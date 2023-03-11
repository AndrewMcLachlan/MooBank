using Asm.MooBank.Data.Entities;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Services
{
    public class SecurityRepository : DataRepository, ISecurityRepository
    {
        private readonly IUserDataProvider _userDataProvider;
        private readonly bool _runningOutOfContext;

        public SecurityRepository(BankPlusContext dataContext, IUserDataProvider userDataProvider, IHttpContextAccessor httpContextAccessor) : base(dataContext)
        {
            _userDataProvider = userDataProvider;

            _runningOutOfContext = httpContextAccessor.HttpContext == null;
        }

        public void AssertPermission(Guid accountId)
        {
            // HACK
            if (_runningOutOfContext) return;

            if (!DataContext.Accounts.Any(a => a.AccountId == accountId && a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))
            {
                throw new NotAuthorisedException("Not authorised to view this account");
            }
        }

        public void AssertPermission(Account account)
        {
            // HACK
            if (_runningOutOfContext) return;

            if (!account.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))
            {
                throw new NotAuthorisedException("Not authorised to view this account");
            }
        }
    }
}
