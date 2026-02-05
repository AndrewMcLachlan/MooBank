#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for instrument (account) authorization.
/// Tests that the authorization pipeline correctly enforces access control.
///
/// Note: Authorization is claim-based - the user's claims contain the account IDs they have access to.
/// The database entity doesn't need to exist for the authorization check itself.
/// </summary>
[Trait("Category", "Integration")]
public class InstrumentAuthorizationTests : IClassFixture<MooBankWebApplicationFactory>
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testAccountId = Guid.NewGuid();
    private readonly Guid _otherAccountId = Guid.NewGuid();
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public InstrumentAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Unauthenticated Access Tests

    [Fact]
    public async Task GetAccount_Unauthenticated_Returns401()
    {
        // Arrange - No auth headers
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}/transactions", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Authorization Denied Tests (403)

    [Fact]
    public async Task GetAccount_AuthenticatedWithNoAccess_Returns403()
    {
        // Arrange - User has no accounts in their claims
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [], // No owned accounts
            SharedAccountIds = [], // No shared accounts
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAccount_AuthenticatedWithDifferentAccount_Returns403()
    {
        // Arrange - User owns a different account
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAccountId], // Owns different account
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAccount_AuthenticatedWithSharedAccessToDifferentAccount_Returns403()
    {
        // Arrange - User has shared access to a different account
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            SharedAccountIds = [_otherAccountId], // Shared access to different account
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests (via claims, may return 404 if entity doesn't exist)

    [Fact]
    public async Task GetAccount_AuthenticatedOwner_PassesAuthorization()
    {
        // Arrange - User owns the account (in their claims)
        var user = TestUser.WithAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert - Should pass authorization (403 would mean auth failed)
        // May return 404 if account doesn't exist in DB, but NOT 403
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAccount_AuthenticatedSharedUser_PassesAuthorization()
    {
        // Arrange - User has shared access to the account (in their claims)
        var user = TestUser.WithSharedAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert - Should pass authorization
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Multi-Account Authorization Tests

    [Fact]
    public async Task GetAccount_UserWithMultipleOwnedAccounts_CanAccessOwned()
    {
        // Arrange - User owns multiple accounts
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_testAccountId, _otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act - Try to access both
        var response1 = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);
        var response2 = await client.GetAsync($"/api/accounts/{_otherAccountId}", TestContext.Current.CancellationToken);

        // Assert - Both should pass auth (not 403)
        Assert.NotEqual(HttpStatusCode.Forbidden, response1.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response2.StatusCode);
    }

    [Fact]
    public async Task GetAccount_UserWithMixedAccess_CorrectPermissions()
    {
        // Arrange - User owns one account and has shared access to another
        var thirdAccountId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_testAccountId],
            SharedAccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var ownedResponse = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);
        var sharedResponse = await client.GetAsync($"/api/accounts/{_otherAccountId}", TestContext.Current.CancellationToken);
        var noAccessResponse = await client.GetAsync($"/api/accounts/{thirdAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, ownedResponse.StatusCode); // Passes auth
        Assert.NotEqual(HttpStatusCode.Forbidden, sharedResponse.StatusCode); // Passes auth
        Assert.Equal(HttpStatusCode.Forbidden, noAccessResponse.StatusCode); // Denied
    }

    #endregion

    #region Admin Authorization Tests

    [Fact]
    public async Task CreateInstitution_NonAdmin_Returns403()
    {
        // Arrange - Regular user (no Admin role)
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [], // No roles
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act - Try to create an institution (admin-only)
        var content = new StringContent("{\"name\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/institutions", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateInstitution_Admin_PassesAuthorization()
    {
        // Arrange - Admin user
        var user = TestUser.Admin();
        var client = _factory.CreateAuthenticatedClient(user);

        // Act - Try to create an institution (admin-only)
        var content = new StringContent("{\"name\":\"Test Institution\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/institutions", content, TestContext.Current.CancellationToken);

        // Assert - Should pass authorization (may fail validation, but NOT 403)
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateInstitution_NonAdmin_Returns403()
    {
        // Arrange - Regular user (no Admin role)
        var institutionId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [], // No roles
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act - Try to update an institution (admin-only)
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/institutions/{institutionId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion
}
