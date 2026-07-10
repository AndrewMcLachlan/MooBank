using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security;

public static class AuthorizationOptionsExtensions
{
    public static void AddPolicies(this AuthorizationOptions options)
    {
        // Note: FamilyMemberRequirement is deliberately NOT registered as a named policy.
        // Its only handler is resource-based (used via ISecurity.AssertFamilyPermission);
        // attaching it to a route would always fail closed.

        options.AddPolicy(Policies.InstrumentOwner, policy =>
        {
            policy.GetInstrumentOwnerPolicy();
        });

        options.AddPolicy(Policies.InstrumentViewer, policy =>
        {
            policy.GetInstrumentViewerPolicy();
        });

        options.AddPolicy(Policies.GroupOwner, policy =>
        {
            policy.GetGroupOwnerPolicy();
        });

        options.AddPolicy(Policies.Admin, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    }
}
