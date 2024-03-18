using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class FamilyMemberAuthorisationHandler(AccountHolder accountHolder) : AuthorizationHandler<FamilyMemberRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FamilyMemberRequirement requirement, Guid resource)
    {
        if (accountHolder.FamilyId == resource)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class FamilyMemberRequirement : IAuthorizationRequirement
{
}
