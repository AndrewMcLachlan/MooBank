using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

public class ForecastPlanRequirement : RouteParamAuthorisationRequirement
{
    public ForecastPlanRequirement() : base("id")
    {
    }

    public ForecastPlanRequirement(string id) : base(id)
    {
    }
}
