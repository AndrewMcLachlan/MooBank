using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class FamilyMemberAuthorisationHandler : AuthorizationHandler<FamilyMemberRequirement, Guid>
{
    private readonly AccountHolder _accountHolder;

    public FamilyMemberAuthorisationHandler(AccountHolder accountHolder)
    {
        _accountHolder = accountHolder;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FamilyMemberRequirement requirement, Guid resource)
    {
        if (_accountHolder.FamilyId == resource)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class FamilyMemberRequirement : IAuthorizationRequirement
{
}