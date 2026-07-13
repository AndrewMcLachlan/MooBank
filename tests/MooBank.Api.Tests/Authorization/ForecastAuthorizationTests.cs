#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;
using Asm.MooBank.Domain.Entities.Forecast;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for forecast-plan authorization (GetForecastPlanPolicy).
/// Exercises the real <c>ForecastPlanAuthorisationHandler</c>, which resolves the plan's family from
/// the database and compares it to the caller's family.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class ForecastAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    private static readonly Guid FamilyId = Guid.NewGuid();
    private static readonly Guid PlanId = Guid.NewGuid();

    private string PlanUrl => $"/api/forecast/plans/{PlanId}";

    private Task SeedPlanAsync() =>
        _factory.SeedDataAsync(async context =>
        {
            if (!await context.Set<ForecastPlan>().IgnoreQueryFilters().AnyAsync(p => p.Id == PlanId))
            {
                context.Add(new ForecastPlan(PlanId) { Name = "Plan", FamilyId = FamilyId });
                await context.SaveChangesAsync();
            }
        });

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a forecast plan
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetPlan_Unauthenticated_Returns401()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(PlanUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the plan
    /// When I request GET for that plan
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetPlan_DifferentFamily_Returns403()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(PlanUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in the plan's family
    /// When I request GET for that plan
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetPlan_SameFamily_PassesAuth()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = FamilyId };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(PlanUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the plan
    /// When I attempt to DELETE that plan
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task DeletePlan_DifferentFamily_Returns403()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.DeleteAsync(PlanUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
