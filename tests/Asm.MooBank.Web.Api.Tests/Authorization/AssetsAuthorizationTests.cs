#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for asset authorization.
/// Tests that the authorization pipeline correctly enforces InstrumentViewer/InstrumentOwner access control for assets.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class AssetsAuthorizationTests
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testAssetId = Guid.NewGuid();
    private readonly Guid _otherAssetId = Guid.NewGuid();
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public AssetsAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Unauthenticated Access Tests

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for an asset
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetAsset_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/assets/{_testAssetId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Authorization Denied Tests (403)

    /// <summary>
    /// Given I am authenticated but own a different asset
    /// When I request GET for an asset I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetAsset_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAssetId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/assets/{_testAssetId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different asset
    /// When I attempt to PATCH an asset I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateAsset_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAssetId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/assets/{_testAssetId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests

    /// <summary>
    /// Given I am authenticated as the asset owner
    /// When I request GET for my owned asset
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetAsset_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testAssetId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/assets/{_testAssetId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access to an asset
    /// When I request GET for the shared asset
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetAsset_SharedUser_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithSharedAccount(_testAssetId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/assets/{_testAssetId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the asset owner
    /// When I request PATCH for my owned asset
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task UpdateAsset_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testAssetId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/assets/{_testAssetId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
