using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class AccountHolderAuthorisationHandler(User user) : AuthorizationHandler<InstrumentOwnerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InstrumentOwnerRequirement requirement, Guid resource)
    {
        if (user.Accounts.Contains(resource))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}


internal class InstrumentOwnerRequirement : IAuthorizationRequirement
{
    public Guid AccountId { get; init; }
}
