using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;

public class TagFamilyRequirement : RouteParamAuthorisationRequirement
{
    public TagFamilyRequirement() : base("id")
    {
    }

    public TagFamilyRequirement(string id) : base(id)
    {
    }
}
