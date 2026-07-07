using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository(MooBankContext mooBankContext, IAuthorizationService authorizationService, IPrincipalProvider principalProvider, User user, IAuditLogger audit) : ISecurity
{
    public void AssertGroupPermission(Guid accountId)
    {
        if (!mooBankContext.Groups.Any(a => a.Id == accountId && a.OwnerId == user.Id))
        {
            audit.AuthorizationDenied(user, "Group", accountId, nameof(AssertGroupPermission));
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public void AssertGroupPermission(Group group)
    {
        if (group.OwnerId != user.Id)
        {
            audit.AuthorizationDenied(user, "Group", group.Id, nameof(AssertGroupPermission));
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public async Task AssertFamilyPermission(Guid familyId)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Family", familyId, nameof(FamilyMemberRequirement));
            throw new NotAuthorisedException("Not authorised to view this family");
        }
    }

    public async Task<bool> HasBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permitted = await mooBankContext.BudgetLines.AnyAsync(bl => bl.Id == id && bl.Budget.FamilyId == user.FamilyId, cancellationToken);

        if (!permitted)
        {
            audit.AuthorizationDenied(user, "BudgetLine", id, nameof(BudgetLineRequirement));
        }

        return permitted;
    }

    public async Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default) =>
        await mooBankContext.InstrumentOwners.Where(aah => aah.UserId == user.Id).Select(aah => aah.InstrumentId).ToListAsync(cancellationToken);

    public async Task AssertAdministrator(CancellationToken cancellationToken = default)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, Policies.Admin);

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Administrator", null, Policies.Admin);
            throw new NotAuthorisedException("Not authorised");
        }
    }
}
