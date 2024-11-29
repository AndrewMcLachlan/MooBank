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
            policy.Requirements.Add(new FamilyMemberRequirement());
        });

        options.AddPolicy(Policies.InstrumentOwner, policy =>
        {
            policy.GetInstrumentOwnerPolicy();
        });

        options.AddPolicy(Policies.InstrumentViewer, policy =>
        {
            policy.GetInstrumentViewerPolicy();
        });

        options.AddPolicy(Policies.Admin, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    }
}
