using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Authorises tag ids that arrive in a request body, where there is no route parameter for
/// <see cref="TagFamilyAuthorisationHandler"/> to read.
/// </summary>
/// <remarks>
/// Fail-closed on absence: an id with no tag behind it is refused rather than ignored, so a
/// mistyped or deleted id cannot be silently dropped from the caller's set.
/// </remarks>
internal class TagFamilyResourceAuthorisationHandler(IAuthorisationReader authorisationReader, User user) : AuthorizationHandler<TagFamilyRequirement, IReadOnlyCollection<int>>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TagFamilyRequirement requirement, IReadOnlyCollection<int> tagIds)
    {
        if (tagIds.Count == 0)
        {
            context.Succeed(requirement);
            return;
        }

        var families = await authorisationReader.GetTagFamilyIds(tagIds);

        if (tagIds.All(id => families.TryGetValue(id, out var familyId) && familyId == user.FamilyId))
        {
            context.Succeed(requirement);
        }
    }
}
