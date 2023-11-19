using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository(MooBankContext dataContext, IAuthorizationService authorizationService, IUserDataProvider userDataProvider, IPrincipalProvider principalProvider) : ISecurity
{
    private readonly IUserDataProvider _userDataProvider = userDataProvider;
    private readonly IPrincipalProvider _principalProvider = principalProvider;
    private readonly MooBankContext _mooBankContext = dataContext;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public void AssertAccountPermission(Guid accountId)
    {
        var virtualAccount = _mooBankContext.VirtualAccounts.Find(accountId);
        var accountToCheck = (virtualAccount != null) ? virtualAccount.InstitutionAccountId : accountId;

        var authResult = _authorizationService.AuthorizeAsync(_principalProvider.Principal!, accountToCheck, Policies.AccountHolder).Result;


        if (!authResult.Succeeded)

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
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountPermission(Account account)
    {
        var authResult = _authorizationService.AuthorizeAsync(_principalProvider.Principal!, account.AccountId, Policies.AccountHolder).Result;

        if (!authResult.Succeeded)
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
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public async Task AssertFamilyPermission(Guid familyId)
    {
        var authResult = await _authorizationService.AuthorizeAsync(_principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this family");
        }
    }

    public async Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var budgetLine = await _mooBankContext.Set<BudgetLine>().Include(b => b.Budget).FindAsync(id, cancellationToken) ?? throw new NotFoundException("Budget line not found");
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
