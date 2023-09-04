using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository : ISecurity
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly IPrincipalProvider _principalProvider;
    private readonly MooBankContext _mooBankContext;
    private readonly IAuthorizationService _authorizationService;

    public SecurityRepository(MooBankContext dataContext, IAuthorizationService authorizationService, IUserDataProvider userDataProvider, IPrincipalProvider principalProvider)
    {
        _userDataProvider = userDataProvider;
        _mooBankContext = dataContext;
        _authorizationService = authorizationService;
        _principalProvider = principalProvider;
    }

    public void AssertAccountPermission(Guid accountId)
    {
        var virtualAccount = _mooBankContext.VirtualAccounts.Find(accountId);
        var accountToCheck = (virtualAccount != null) ? virtualAccount.InstitutionAccountId : accountId;

        var authResult = _authorizationService.AuthorizeAsync(_principalProvider.Principal!, accountToCheck, Policies.AccountHolder).Result;


        if (!authResult.Succeeded)
        //if (! _mooBankContext.Accounts.Any(a => a.AccountId == accountToCheck && a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))

        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public async Task AssertAccountPermissionAsync(Guid accountId)
    {
        var virtualAccount = await _mooBankContext.VirtualAccounts.FindAsync(accountId);

        var accountToCheck = (virtualAccount != null) ? virtualAccount.InstitutionAccountId : accountId;

        var authResult = await _authorizationService.AuthorizeAsync(_principalProvider.Principal!, accountToCheck, Policies.AccountHolder);

        if (!authResult.Succeeded)
        //if (!(await _mooBankContext.Accounts.AnyAsync(a => a.AccountId == accountToCheck && a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountPermission(Account account)
    {
        var authResult = _authorizationService.AuthorizeAsync(_principalProvider.Principal!, account.AccountId, Policies.AccountHolder).Result;

        if (!account.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountGroupPermission(Guid accountGroupId)
    {
        if (!_mooBankContext.AccountGroups.Any(a => a.Id == accountGroupId && a.OwnerId == _userDataProvider.CurrentUserId))
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

    public async Task AssertFamilyPermission(Guid familyId)
    {
        var authResult = await _authorizationService.AuthorizeAsync(_principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public async Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var budgetLine = await _mooBankContext.Set<BudgetLine>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken) ?? throw new NotFoundException("Budget line not found");
        var authResult = await _authorizationService.AuthorizeAsync(_principalProvider.Principal!, budgetLine.Budget.FamilyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this budget line");
        }
    }

    public async Task<IEnumerable<Guid>> GetAccountIds(CancellationToken cancellationToken = default) =>
        await _mooBankContext.AccountAccountHolder.Where(aah => aah.AccountHolderId == _userDataProvider.CurrentUserId).Select(aah => aah.AccountId).ToListAsync(cancellationToken);

    public void AssertAdministrator()
    {
    }
}
