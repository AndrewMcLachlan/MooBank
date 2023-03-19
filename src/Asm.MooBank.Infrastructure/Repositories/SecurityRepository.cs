using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Repositories;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository : ISecurityRepository
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly bool _runningOutOfContext;
    private readonly BankPlusContext _bankPlusContext;

    public SecurityRepository(BankPlusContext dataContext, IUserDataProvider userDataProvider, IHttpContextAccessor httpContextAccessor)
    {
        _userDataProvider = userDataProvider;
        _runningOutOfContext = httpContextAccessor.HttpContext == null;
        _bankPlusContext = dataContext;
    }

    public void AssertPermission(Guid accountId)
    {
        // HACK
        if (_runningOutOfContext) return;

        if (!_bankPlusContext.Accounts.Any(a => a.AccountId == accountId && a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))
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
