using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class AccountViewerAuthorisationHandler(User user) : AuthorizationHandler<InstrumentViewerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InstrumentViewerRequirement requirement, Guid resource)
    {
        if (user.Accounts.Contains(resource) || user.SharedAccounts.Contains(resource))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}


internal class InstrumentViewerRequirement : IAuthorizationRequirement
{
    public Guid AccountId { get; init; }
}
