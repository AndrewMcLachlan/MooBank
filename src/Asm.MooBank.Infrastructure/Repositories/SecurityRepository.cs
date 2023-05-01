using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository : ISecurityRepository
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly bool _runningOutOfContext;
    private readonly MooBankContext _bankPlusContext;

    public SecurityRepository(MooBankContext dataContext, IUserDataProvider userDataProvider, IHttpContextAccessor httpContextAccessor)
    {
        _userDataProvider = userDataProvider;
        _runningOutOfContext = httpContextAccessor.HttpContext == null;
        _bankPlusContext = dataContext;
    }

    public void AssertAccountPermission(Guid accountId)
    {
        // HACK
        if (_runningOutOfContext) return;

        if (!_bankPlusContext.Accounts.Any(a => a.AccountId == accountId && a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountPermission(Account account)
    {
        // HACK
        if (_runningOutOfContext) return;

        if (!account.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountGroupPermission(Guid accountGroupId)
    {

        if (!_bankPlusContext.AccountGroups.Any(a => a.Id == accountGroupId && a.OwnerId == _userDataProvider.CurrentUserId))
        {
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public void AssertAccountGroupPermission(AccountGroup accountGroup)
    {
        if (accountGroup.OwnerId != _userDataProvider.CurrentUserId)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }
}
