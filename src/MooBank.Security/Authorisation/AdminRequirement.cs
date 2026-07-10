using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

public class AdminRequirement : IAuthorizationRequirement
{
    public const string RoleName = "Admin";
}
