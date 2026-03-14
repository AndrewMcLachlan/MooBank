using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentViewerResourceAuthorisationHandler(User user) : AuthorizationHandler<InstrumentViewerRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InstrumentViewerRequirement requirement, Guid instrumentId)
    {
        if (user.Accounts.Contains(instrumentId) || user.SharedAccounts.Contains(instrumentId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
