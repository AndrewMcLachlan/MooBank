using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

public abstract class RoleRequirement : IAuthorizationRequirement
{
    public abstract string RoleName { get; }

    public abstract string RoleDescription { get; }
}
