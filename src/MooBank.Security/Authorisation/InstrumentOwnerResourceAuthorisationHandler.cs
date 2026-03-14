using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentOwnerResourceAuthorisationHandler(User user) : AuthorizationHandler<InstrumentOwnerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InstrumentOwnerRequirement requirement, Guid instrumentId)
    {
        if (user.Accounts.Contains(instrumentId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
