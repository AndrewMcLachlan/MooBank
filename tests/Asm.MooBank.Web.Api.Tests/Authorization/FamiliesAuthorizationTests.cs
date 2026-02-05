#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for families admin authorization.
/// Tests that the authorization pipeline correctly enforces Admin role for family admin endpoints.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class FamiliesAuthorizationTests
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public FamiliesAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Authorization Denied Tests (403)

    /// <summary>
    /// Given I am authenticated as a regular user without admin role
    /// When I attempt to POST a new family via the admin endpoint
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task CreateFamily_NonAdmin_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"New Family\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/families/admin", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as a regular user without admin role
    /// When I attempt to PATCH a family via the admin endpoint
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateFamily_NonAdmin_Returns403()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/families/admin/{familyId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests

    /// <summary>
    /// Given I am authenticated as an admin user
    /// When I request POST to create a family via the admin endpoint
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task CreateFamily_Admin_PassesAuth()
    {
        // Arrange
        var user = TestUser.Admin();
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"New Family\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/families/admin", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as an admin user
    /// When I request PATCH to update a family via the admin endpoint
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task UpdateFamily_Admin_PassesAuth()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var user = TestUser.Admin();
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/families/admin/{familyId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
