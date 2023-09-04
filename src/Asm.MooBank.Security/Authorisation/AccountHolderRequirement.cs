using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

internal class AccountHolderAuthorisationHandler : AuthorizationHandler<AccountHolderRequirement, Guid>
{
    private readonly AccountHolder _accountHolder;

    public AccountHolderAuthorisationHandler(AccountHolder accountHolder)
    {
        _accountHolder = accountHolder;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountHolderRequirement requirement, Guid resource)
    {
        if (_accountHolder.Accounts.Contains(resource))
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