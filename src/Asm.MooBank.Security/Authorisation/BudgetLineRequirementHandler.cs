using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;


internal class BudgetLineAuthorisationHandler : AuthorizationHandler<BudgetLineRequirement, Domain.Entities.Budget.BudgetLine>
{
    private readonly IUserDataProvider _userDataProvider;

    public BudgetLineAuthorisationHandler(IUserDataProvider userDataProvider)
    {
        _userDataProvider = userDataProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BudgetLineRequirement requirement, Domain.Entities.Budget.BudgetLine resource)
    {
        var user = await _userDataProvider.GetCurrentUserAsync();
        if (user == null) return;

        if (resource.Budget.FamilyId == user.FamilyId)
        {
            context.Succeed(requirement);
        }
    }
}

public class BudgetLineRequirement : IAuthorizationRequirement
{
}
