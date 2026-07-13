#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;
using Asm.MooBank.Domain.Entities.Budget;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for budget-line authorization (GetBudgetLinePolicy).
/// Exercises the real <c>BudgetLineAuthorisationHandler</c>, which resolves the line's family (via its
/// budget) from the database and compares it to the caller's family.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class BudgetsAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    private static readonly Guid FamilyId = Guid.NewGuid();
    private static readonly Guid BudgetId = Guid.NewGuid();
    private static readonly Guid LineId = Guid.NewGuid();
    private const short Year = 2026;

    private string LineUrl => $"/api/budget/{Year}/lines/{LineId}";

    private Task SeedBudgetLineAsync() =>
        _factory.SeedDataAsync(async context =>
        {
            if (!await context.Set<BudgetLine>().IgnoreQueryFilters().AnyAsync(bl => bl.Id == LineId))
            {
                context.Add(new Budget(BudgetId) { Year = Year, FamilyId = FamilyId });
                context.Add(new BudgetLine(LineId) { BudgetId = BudgetId, TagId = 1, Amount = 100m });
                await context.SaveChangesAsync();
            }
        });

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a budget line
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetBudgetLine_Unauthenticated_Returns401()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(LineUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the budget line
    /// When I request GET for that line
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetBudgetLine_DifferentFamily_Returns403()
    {
        await SeedBudgetLineAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(LineUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in the budget line's family
    /// When I request GET for that line
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetBudgetLine_SameFamily_PassesAuth()
    {
        await SeedBudgetLineAsync();
        var user = new TestUser { FamilyId = FamilyId };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(LineUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the budget line
    /// When I attempt to DELETE that line
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task DeleteBudgetLine_DifferentFamily_Returns403()
    {
        await SeedBudgetLineAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.DeleteAsync(LineUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
