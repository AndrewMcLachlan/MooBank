
using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security;

public static class Policies
{
    public const string Admin = nameof(Admin);

    public const string FamilyMember = nameof(FamilyMember);

    public const string InstrumentOwner = nameof(InstrumentOwner);

    public const string InstrumentViewer = nameof(InstrumentViewer);

    public const string GroupOwner = nameof(GroupOwner);

    public const string BudgetLine = nameof(BudgetLine);

    public static AuthorizationPolicy GetInstrumentOwnerPolicy(string routeParam = "instrumentId") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetInstrumentOwnerPolicy(routeParam);

    public static AuthorizationPolicy GetInstrumentOwnerPolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "instrumentId")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new InstrumentOwnerRequirement(routeParam));

        return policyBuilder.Build();
    }

    public static AuthorizationPolicy GetInstrumentViewerPolicy(string routeParam = "instrumentId") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetInstrumentViewerPolicy(routeParam);

    public static AuthorizationPolicy GetInstrumentViewerPolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "instrumentId")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new InstrumentViewerRequirement(routeParam));

        return policyBuilder.Build();
    }

    public static AuthorizationPolicy GetGroupOwnerPolicy(string routeParam = "groupId") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetGroupOwnerPolicy(routeParam);

    public static AuthorizationPolicy GetGroupOwnerPolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "groupId")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new GroupOwnerRequirement(routeParam));

        return policyBuilder.Build();
    }
}
