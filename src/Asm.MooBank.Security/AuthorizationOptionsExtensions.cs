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

        options.AddPolicy(Policies.InstrumentOwner, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.AccountId);
            policy.Requirements.Add(new InstrumentOwnerRequirement());
        });

        options.AddPolicy(Policies.InstrumentViewer, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimTypes.AccountId);
            policy.Requirements.Add(new InstrumentViewerRequirement());
        });

        options.AddPolicy(Policies.Admin, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    }
}
