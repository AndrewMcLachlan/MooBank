using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;


internal class BudgetLineAuthorisationHandler(User accountHolder) : AuthorizationHandler<BudgetLineRequirement, Domain.Entities.Budget.BudgetLine>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BudgetLineRequirement requirement, Domain.Entities.Budget.BudgetLine resource)
    {
        if (resource.Budget.FamilyId == accountHolder.FamilyId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class BudgetLineRequirement : IAuthorizationRequirement
{
}
