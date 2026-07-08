using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class GroupOwnerResourceAuthorisationHandler(IAuthorisationRepository authorisationRepository, User user) : AuthorizationHandler<GroupOwnerRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupOwnerRequirement requirement, Guid groupId)
    {
        if (await authorisationRepository.IsGroupOwner(groupId, user.Id))
        {
            context.Succeed(requirement);
        }
    }
}
