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
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class InstrumentAuthorizationTests
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

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for an account
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetAccount_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for transactions
    /// Then the response status should be 401 Unauthorized
    /// </summary>
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

    /// <summary>
    /// Given I am authenticated with no accounts in my claims
    /// When I request GET for an account
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetAccount_AuthenticatedWithNoAccess_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [],
            SharedAccountIds = [],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different account
    /// When I request GET for an account I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetAccount_AuthenticatedWithDifferentAccount_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access to a different account
    /// When I request GET for an account I don't have access to
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetAccount_AuthenticatedWithSharedAccessToDifferentAccount_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            SharedAccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests (via claims, may return 404 if entity doesn't exist)

    /// <summary>
    /// Given I am authenticated as the account owner
    /// When I request GET for my owned account
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetAccount_AuthenticatedOwner_PassesAuthorization()
    {
        // Arrange
        var user = TestUser.WithAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access to the account
    /// When I request GET for the shared account
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetAccount_AuthenticatedSharedUser_PassesAuthorization()
    {
        // Arrange
        var user = TestUser.WithSharedAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Multi-Account Authorization Tests

    /// <summary>
    /// Given I am authenticated with multiple owned accounts
    /// When I request GET for each owned account
    /// Then authorization should pass for all owned accounts
    /// </summary>
    [Fact]
    public async Task GetAccount_UserWithMultipleOwnedAccounts_CanAccessOwned()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_testAccountId, _otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response1 = await client.GetAsync($"/api/accounts/{_testAccountId}", TestContext.Current.CancellationToken);
        var response2 = await client.GetAsync($"/api/accounts/{_otherAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response1.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response2.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with owned and shared access to different accounts
    /// When I request GET for owned, shared, and unauthorized accounts
    /// Then authorization should pass for owned and shared, but deny for unauthorized
    /// </summary>
    [Fact]
    public async Task GetAccount_UserWithMixedAccess_CorrectPermissions()
    {
        // Arrange
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
        Assert.NotEqual(HttpStatusCode.Forbidden, ownedResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, sharedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, noAccessResponse.StatusCode);
    }

    #endregion

    #region Admin Authorization Tests

    /// <summary>
    /// Given I am authenticated as a regular user without admin role
    /// When I attempt to POST a new institution
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task CreateInstitution_NonAdmin_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/institutions", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as an admin user
    /// When I request POST to create an institution
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task CreateInstitution_Admin_PassesAuthorization()
    {
        // Arrange
        var user = TestUser.Admin();
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Test Institution\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/institutions", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as a regular user without admin role
    /// When I attempt to PATCH an institution
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task UpdateInstitution_NonAdmin_Returns403()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            Roles = [],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Updated\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/institutions/{institutionId}", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Sub-Endpoint Authorization Tests

    /// <summary>
    /// Given I am authenticated but own a different account
    /// When I request GET for an institution account I don't have access to
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetInstitutionAccount_NonOwner_Returns403()
    {
        // Arrange
        var institutionAccountId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}/institution-accounts/{institutionAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different account
    /// When I attempt to POST a recurring transaction for an account I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task CreateRecurringTransaction_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"description\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/accounts/{_testAccountId}/recurring", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but own a different account
    /// When I request GET for virtual recurring transactions for an account I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetVirtualRecurring_NonOwner_Returns403()
    {
        // Arrange
        var virtualAccountId = Guid.NewGuid();
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherAccountId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}/virtual/{virtualAccountId}/recurring", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the account owner
    /// When I request GET for an institution account
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetInstitutionAccount_Owner_PassesAuth()
    {
        // Arrange
        var institutionAccountId = Guid.NewGuid();
        var user = TestUser.WithAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var response = await client.GetAsync($"/api/accounts/{_testAccountId}/institution-accounts/{institutionAccountId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the account owner
    /// When I request POST to create a recurring transaction
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task CreateRecurringTransaction_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testAccountId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"description\":\"Test\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/accounts/{_testAccountId}/recurring", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
