using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Models;
using Asm.MooBank.Security.Authorisation;
using Asm.Security;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security;

public class Security(IAuthorizationService authorizationService, IPrincipalProvider principalProvider, User user, IAuditLogger audit) : ISecurity
{
    public async Task AssertGroupPermission(Guid groupId)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, groupId, new GroupOwnerRequirement());

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Group", groupId, nameof(GroupOwnerRequirement));
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public Task AssertGroupPermission(Group group) => AssertGroupPermission(group.Id);

    public async Task AssertFamilyPermission(Guid familyId)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, familyId, new FamilyMemberRequirement());

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Family", familyId, nameof(FamilyMemberRequirement));
            throw new NotAuthorisedException("Not authorised to view this family");
        }
    }

    public async Task AssertTagPermission(IReadOnlyCollection<int> tagIds)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, tagIds, new TagFamilyRequirement());

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Tag", String.Join(", ", tagIds), nameof(TagFamilyRequirement));
            throw new NotAuthorisedException("Not authorised to use one or more of these tags.");
        }
    }

    public async Task AssertInstrumentViewer(Guid instrumentId)
    {
        var authResult = await authorizationService.AuthorizeAsync(principalProvider.Principal!, instrumentId, new InstrumentViewerRequirement());

        if (!authResult.Succeeded)
        {
            audit.AuthorizationDenied(user, "Instrument", instrumentId, nameof(InstrumentViewerRequirement));
            throw new NotAuthorisedException("Not authorised to view this instrument.");
        }
    }

}
