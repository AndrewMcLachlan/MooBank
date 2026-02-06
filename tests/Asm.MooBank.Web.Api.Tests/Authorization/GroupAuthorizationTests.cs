#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for group authorization.
/// Tests that the authorization pipeline correctly enforces GroupOwner access control.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class GroupAuthorizationTests
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testGroupId = Guid.NewGuid();
    private readonly Guid _otherGroupId = Guid.NewGuid();
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public GroupAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Unauthenticated Access Tests

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a group
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetGroup_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/groups/{_testGroupId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Authorization Denied Tests (403)

    /// <summary>
    /// Given I am authenticated but own a different group
    /// When I request GET for a group I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetGroup_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            GroupIds = [_otherGroupId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/groups/{_testGroupId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different group
    /// When I attempt to PATCH a group I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateGroup_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            GroupIds = [_otherGroupId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/groups/{_testGroupId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different group
    /// When I attempt to DELETE a group I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task DeleteGroup_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            GroupIds = [_otherGroupId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.DeleteAsync($"/api/groups/{_testGroupId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests

    /// <summary>
    /// Given I am authenticated as the group owner
    /// When I request GET for my owned group
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetGroup_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithGroup(_testGroupId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/groups/{_testGroupId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the group owner
    /// When I request PATCH for my owned group
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task UpdateGroup_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithGroup(_testGroupId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/groups/{_testGroupId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the group owner
    /// When I request DELETE for my owned group
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task DeleteGroup_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithGroup(_testGroupId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.DeleteAsync($"/api/groups/{_testGroupId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated
    /// When I request POST to create a new group
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task CreateGroup_Authenticated_PassesAuth()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"New Group\",\"description\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/groups", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
