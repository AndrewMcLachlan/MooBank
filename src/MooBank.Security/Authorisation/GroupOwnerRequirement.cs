using Asm.AspNetCore.Authorisation;

namespace Asm.MooBank.Security.Authorisation;


internal class GroupOwnerRequirement : RouteParamAuthorisationRequirement
{
    public GroupOwnerRequirement() : base("groupId")
    {
    }

    public GroupOwnerRequirement(string id) : base(id)
    {
    }
}
