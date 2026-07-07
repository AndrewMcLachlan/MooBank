using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

public class BudgetLineRequirement : RouteParamAuthorisationRequirement
{
    public BudgetLineRequirement() : base("id")
    {
    }

    public BudgetLineRequirement(string id) : base(id)
    {
    }
}
