
using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security;

public static class Policies
{
    public const string Admin = nameof(Admin);

    public const string InstrumentOwner = nameof(InstrumentOwner);

    public const string InstrumentViewer = nameof(InstrumentViewer);

    public const string GroupOwner = nameof(GroupOwner);

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

    public static AuthorizationPolicy GetBudgetLinePolicy(string routeParam = "id") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetBudgetLinePolicy(routeParam);

    public static AuthorizationPolicy GetBudgetLinePolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "id")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new BudgetLineRequirement(routeParam));

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

    public static AuthorizationPolicy GetTagFamilyPolicy(string routeParam = "id") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetTagFamilyPolicy(routeParam);

    public static AuthorizationPolicy GetTagFamilyPolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "id")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new TagFamilyRequirement(routeParam));

        return policyBuilder.Build();
    }

    public static AuthorizationPolicy GetForecastPlanPolicy(string routeParam = "id") =>
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).GetForecastPlanPolicy(routeParam);

    public static AuthorizationPolicy GetForecastPlanPolicy(this AuthorizationPolicyBuilder policyBuilder, string routeParam = "id")
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddRequirements(new ForecastPlanRequirement(routeParam));

        return policyBuilder.Build();
    }
}
