using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class AccountHolderAuthorisationHandler(AccountHolder accountHolder) : AuthorizationHandler<AccountHolderRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountHolderRequirement requirement, Guid resource)
    {
        if (accountHolder.Accounts.Contains(resource))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}


internal class AccountHolderRequirement : IAuthorizationRequirement
{
    public Guid AccountId { get; init; }
}
