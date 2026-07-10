using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Backs the Admin policy with a requirement so that denials are audited,
/// unlike a bare <c>RequireRole</c> check.
/// </summary>
internal class AdminAuthorisationHandler(User? user, IAuditLogger audit) : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.IsInRole(AdminRequirement.RoleName))
        {
            context.Succeed(requirement);
        }
        else if (user is not null)
        {
            audit.AuthorizationDenied(user, "Administrator", null, nameof(AdminRequirement));
        }

        return Task.CompletedTask;
    }
}
