using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

public class RetirementPlanRequirement : RouteParamAuthorisationRequirement
{
    public RetirementPlanRequirement() : base("id")
    {
    }

    public RetirementPlanRequirement(string id) : base(id)
    {
    }
}
