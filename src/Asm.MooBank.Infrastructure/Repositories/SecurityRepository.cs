using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository(MooBankContext mooBankContext, IAuthorizationService authorizationService, IPrincipalProvider principalProvider, User user) : ISecurity
{
    public void AssertInstrumentPermission(Guid instrumentId)
    {
        var virtualAccount = mooBankContext.VirtualAccounts.Find(instrumentId);
        var instrumentToCheck = (virtualAccount != null) ? virtualAccount.ParentInstrumentId : instrumentId;

        var authResult = authorizationService.AuthorizeAsync(principalProvider.Principal!, instrumentToCheck, Policies.InstrumentViewer).Result;

        if (!authResult.Succeeded)

        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public async Task AssertInstrumentPermissionAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        var virtualAccount = await mooBankContext.VirtualAccounts.FindAsync(instrumentId, cancellationToken);

        var instrumentToCheck = (virtualAccount != null) ? virtualAccount.Id : instrumentId;

        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, instrumentToCheck, Policies.InstrumentViewer);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertInstrumentPermission(Domain.Entities.Instrument.Instrument instrument)
    {
        var authResult = authorizationService.AuthorizeAsync(principalProvider.Principal!, instrument.Id, Policies.InstrumentViewer).Result;

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertGroupPermission(Guid accountId)
    {
        if (!mooBankContext.Groups.Any(a => a.Id == accountId && a.OwnerId == user.Id))
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
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this family");
        }
    }

    public async Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var budgetLine = await mooBankContext.Set<BudgetLine>().Include(b => b.Budget).FindAsync(id, cancellationToken) ?? throw new NotFoundException("Budget line not found");
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, budgetLine.Budget.FamilyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised to view this budget line");
        }
    }

    public async Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default) =>
        await mooBankContext.InstrumentOwners.Where(aah => aah.UserId == user.Id).Select(aah => aah.InstrumentId).ToListAsync(cancellationToken);

    public async Task AssertAdministrator(CancellationToken cancellationToken = default)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, Policies.Admin);

        if (!authResult.Succeeded)
        {
            throw new NotAuthorisedException("Not authorised");
        }
    }
}
