#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for stock authorization.
/// Tests that the authorization pipeline correctly enforces InstrumentViewer/InstrumentOwner access control for stocks.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class StocksAuthorizationTests
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testStockId = Guid.NewGuid();
    private readonly Guid _otherStockId = Guid.NewGuid();
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public StocksAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Unauthenticated Access Tests

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for a stock
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetStock_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Authorization Denied Tests (403)

    /// <summary>
    /// Given I am authenticated but own a different stock
    /// When I request GET for a stock I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetStock_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherStockId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different stock
    /// When I attempt to PATCH a stock I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateStock_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherStockId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/stocks/{_testStockId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different stock
    /// When I request GET for a stock report I don't have access to
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetStockReport_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherStockId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}/reports/value", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests

    /// <summary>
    /// Given I am authenticated as the stock owner
    /// When I request GET for my owned stock
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetStock_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testStockId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access to a stock
    /// When I request GET for the shared stock
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetStock_SharedUser_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithSharedAccount(_testStockId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the stock owner
    /// When I request GET for the stock report
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetStockReport_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testStockId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/stocks/{_testStockId}/reports/value", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
