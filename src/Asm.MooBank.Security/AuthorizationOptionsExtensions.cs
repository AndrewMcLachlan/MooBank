using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security;

public static class AuthorizationOptionsExtensions
{
    public static void AddPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Policies.FamilyMember, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.FamilyId);
            policy.Requirements.Add(new FamilyMemberRequirement());
        });

        options.AddPolicy(Policies.AccountHolder, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.AccountId);
            policy.Requirements.Add(new AccountHolderRequirement());
        });

        options.AddPolicy(Policies.AccountViewer, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.AccountId);
            policy.Requirements.Add(new AccountViewerRequirement());
        });

        options.AddPolicy(Policies.Admin, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    }
}
