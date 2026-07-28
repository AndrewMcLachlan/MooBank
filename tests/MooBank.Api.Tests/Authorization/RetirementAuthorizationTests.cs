#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;
using Asm.MooBank.Domain.Entities.Retirement;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for retirement-plan authorization (GetRetirementPlanPolicy).
/// Exercises the real <c>RetirementPlanAuthorisationHandler</c>, which resolves the plan's family
/// from the database and compares it to the caller's family.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class RetirementAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    private static readonly Guid FamilyId = Guid.NewGuid();
    private static readonly Guid PlanId = Guid.NewGuid();

    private string PlanUrl => $"/api/retirement/plans/{PlanId}";

    private string RunUrl => $"/api/retirement/plans/{PlanId}/run";

    private Task SeedPlanAsync() =>
        _factory.SeedDataAsync(async context =>
        {
            if (!await context.Set<RetirementPlan>().IgnoreQueryFilters().AnyAsync(p => p.Id == PlanId))
            {
                context.Add(new RetirementPlan(PlanId)
                {
                    Name = "Plan",
                    FamilyId = FamilyId,
                    ExpectedReturnRate = 0.065m,
                    InflationRate = 0.025m,
                    SuperGuaranteeRate = 0.12m,
                    ContributionsTaxRate = 0.15m,
                    LifeExpectancy = 90,
                });
                await context.SaveChangesAsync();
            }
        });

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a retirement plan
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
    /// Then the plan should be returned
    /// </summary>
    /// <remarks>
    /// Asserts 200 rather than merely "not forbidden": a 500 from a missing registration would
    /// satisfy the weaker check, so this doubles as proof that the module's services, the EF model
    /// and the query handler are all wired up.
    /// </remarks>
    [Fact]
    public async Task GetPlan_SameFamily_ReturnsThePlan()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = FamilyId };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(PlanUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    /// <summary>
    /// Given I am authenticated in a different family from the plan
    /// When I attempt to run its projection
    /// Then the response status should be 403 Forbidden
    /// </summary>
    /// <remarks>
    /// The run endpoint takes the plan id in a differently-named route parameter, so it needs its
    /// own coverage: a policy bound to the wrong parameter name would let this through.
    /// </remarks>
    [Fact]
    public async Task RunProjection_DifferentFamily_Returns403()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.PostAsync(RunUrl, null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in the plan's family
    /// When I run its projection
    /// Then a projection should be returned
    /// </summary>
    /// <remarks>
    /// The seeded plan has no members, so this exercises the engine's empty case end to end
    /// through the real host.
    /// </remarks>
    [Fact]
    public async Task RunProjection_SameFamily_ReturnsAProjection()
    {
        await SeedPlanAsync();
        var user = new TestUser { FamilyId = FamilyId };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.PostAsync(RunUrl, null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
