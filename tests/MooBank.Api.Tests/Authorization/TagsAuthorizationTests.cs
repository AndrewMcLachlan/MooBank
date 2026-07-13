#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;
using Asm.MooBank.Domain.Entities.Tag;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for tag authorization (GetTagFamilyPolicy).
/// Exercises the real <c>TagFamilyAuthorisationHandler</c>, which resolves the tag's family from the
/// database and compares it to the caller's family.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class TagsAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    // Static so every test method (xUnit constructs a new class instance per test) agrees on the
    // seeded tag's family in the shared in-memory database. TagId is likewise unique across the
    // Authorization collection.
    private static readonly Guid FamilyId = Guid.NewGuid();
    private const int TagId = 987_001;

    private Task SeedTagAsync() =>
        _factory.SeedDataAsync(async context =>
        {
            if (!await context.Set<Tag>().IgnoreQueryFilters().AnyAsync(t => t.Id == TagId))
            {
                context.Add(new Tag(TagId) { Name = "Groceries", FamilyId = FamilyId });
                await context.SaveChangesAsync();
            }
        });

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a tag
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetTag_Unauthenticated_Returns401()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync($"/api/tags/{TagId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the tag
    /// When I request GET for that tag
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetTag_DifferentFamily_Returns403()
    {
        await SeedTagAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync($"/api/tags/{TagId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in the tag's family
    /// When I request GET for that tag
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetTag_SameFamily_PassesAuth()
    {
        await SeedTagAsync();
        var user = new TestUser { FamilyId = FamilyId };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync($"/api/tags/{TagId}", TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the tag
    /// When I attempt to PATCH that tag
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateTag_DifferentFamily_Returns403()
    {
        await SeedTagAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var content = new StringContent("{\"name\":\"Renamed\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/tags/{TagId}", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated in a different family from the tag
    /// When I attempt to DELETE that tag
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task DeleteTag_DifferentFamily_Returns403()
    {
        await SeedTagAsync();
        var user = new TestUser { FamilyId = Guid.NewGuid() };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.DeleteAsync($"/api/tags/{TagId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
