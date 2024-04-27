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

public class SecurityRepository(MooBankContext dataContext, IAuthorizationService authorizationService, IPrincipalProvider principalProvider, User user) : ISecurity
{
    private readonly MooBankContext _mooBankContext = dataContext;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public void AssertInstrumentPermission(Guid instrumentId)
    {
        var virtualAccount = _mooBankContext.VirtualAccounts.Find(instrumentId);
        var instrumentToCheck = (virtualAccount != null) ? virtualAccount.ParentInstrumentId : instrumentId;

        var authResult = _authorizationService.AuthorizeAsync(principalProvider.Principal!, instrumentToCheck, Policies.InstrumentViewer).Result;

        if (!authResult.Succeeded)

        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public async Task AssertInstrumentPermissionAsync(Guid instrumentId)
    {
        var virtualAccount = await _mooBankContext.VirtualAccounts.FindAsync(instrumentId);

        var instrumentToCheck = (virtualAccount != null) ? virtualAccount.Id : instrumentId;

        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, instrumentToCheck, Policies.InstrumentViewer);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertInstrumentPermission(Instrument instrument)
    {
        var authResult = _authorizationService.AuthorizeAsync(principalProvider.Principal!, instrument.Id, Policies.InstrumentViewer).Result;

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertGroupPermission(Guid accountId)
    {
        if (!_mooBankContext.Groups.Any(a => a.Id == accountId && a.OwnerId == user.Id))
        {
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public void AssertGroupPermission(Group group)
    {
        if (group.OwnerId != user.Id)
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

    public async Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default) =>
        await _mooBankContext.InstrumentOwners.Where(aah => aah.UserId == user.Id).Select(aah => aah.InstrumentId).ToListAsync(cancellationToken);

    public async Task AssertAdministrator(CancellationToken cancellationToken = default)
    {
        var authResult = await _authorizationService.AuthorizeAsync(principalProvider.Principal!, Policies.Admin);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised");
        }
    }
}
