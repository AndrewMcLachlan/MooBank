using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Backs the policy with a role requirement so that denials are audited,
/// unlike a bare <c>RequireRole</c> check.
/// </summary>
internal class RoleAuthorisationHandler(User? user, IAuditLogger audit) : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.IsInRole(requirement.RoleName))
        {
            context.Succeed(requirement);
        }
        else if (user is not null)
        {
            audit.AuthorizationDenied(user, requirement.RoleDescription, null, nameof(RoleRequirement));
        }

        return Task.CompletedTask;
    }
}
