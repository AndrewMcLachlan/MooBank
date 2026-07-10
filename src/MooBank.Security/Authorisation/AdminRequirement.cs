using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

public class AdminRequirement : RoleRequirement
{
    public const string AdminRoleName = "Admin";

    public override string RoleName => AdminRoleName;

    public override string RoleDescription => "Administrator";
}
