using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class GroupOwnerResourceAuthorisationHandler(IAuthorisationReader authorisationReader, User user) : AuthorizationHandler<GroupOwnerRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupOwnerRequirement requirement, Guid groupId)
    {
        if (await authorisationReader.IsGroupOwner(groupId, user.Id))
        {
            context.Succeed(requirement);
        }
    }
}
