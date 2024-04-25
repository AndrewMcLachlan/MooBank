using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository(MooBankContext dataContext, IAuthorizationService authorizationService, IPrincipalProvider principalProvider, AccountHolder accountHolder) : ISecurity
{
    private readonly MooBankContext _mooBankContext = dataContext;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public void AssertAccountPermission(Guid accountId)
    {
        var virtualAccount = _mooBankContext.VirtualAccounts.Find(accountId);
        var accountToCheck = (virtualAccount != null) ? virtualAccount.ParentInstrumentId : accountId;

        var authResult = _authorizationService.AuthorizeAsync(principalProvider.Principal!, accountToCheck, Policies.AccountViewer).Result;

        if (!authResult.Succeeded)

        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public async Task AssertAccountPermissionAsync(Guid accountId)
    {
        var virtualAccount = await _mooBankContext.VirtualAccounts.FindAsync(accountId);

        var accountToCheck = (virtualAccount != null) ? virtualAccount.Id : accountId;

        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, accountToCheck, Policies.AccountViewer);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountPermission(Instrument account)
    {
        var authResult = _authorizationService.AuthorizeAsync(principalProvider.Principal!, account.Id, Policies.AccountViewer).Result;

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountGroupPermission(Guid accountGroupId)
    {
        if (!_mooBankContext.Groups.Any(a => a.Id == accountGroupId && a.OwnerId == accountHolder.Id))
        {
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public void AssertAccountGroupPermission(Group accountGroup)
    {
        if (accountGroup.OwnerId != accountHolder.Id)
        {
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public async Task AssertFamilyPermission(Guid familyId)
    {
        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this family");
        }
    }

    public async Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var budgetLine = await _mooBankContext.Set<BudgetLine>().Include(b => b.Budget).FindAsync(id, cancellationToken) ?? throw new NotFoundException("Budget line not found");
        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, budgetLine.Budget.FamilyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this budget line");
        }
    }

    public async Task<IEnumerable<Guid>> GetAccountIds(CancellationToken cancellationToken = default) =>
        await _mooBankContext.InstrumentOwners.Where(aah => aah.UserId == accountHolder.Id).Select(aah => aah.InstrumentId).ToListAsync(cancellationToken);

    public async Task AssertAdministrator(CancellationToken cancellationToken = default)
    {
        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, Policies.Admin);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised");
        }
    }
}
