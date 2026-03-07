#nullable enable
using Asm.MooBank.Web.Api.Tests.Infrastructure;

namespace Asm.MooBank.Web.Api.Tests.Authorization;

/// <summary>
/// Integration tests for bill authorization.
/// Tests that the authorization pipeline correctly enforces InstrumentOwner access control for bills.
/// Note: Bills require InstrumentOwner, not just InstrumentViewer.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class BillsAuthorizationTests
{
    private readonly MooBankWebApplicationFactory _factory;
    private readonly Guid _testInstrumentId = Guid.NewGuid();
    private readonly Guid _otherInstrumentId = Guid.NewGuid();
    private readonly Guid _testFamilyId = Guid.NewGuid();

    public BillsAuthorizationTests(MooBankWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Unauthenticated Access Tests

    /// <summary>
    /// Given I am not authenticated
    /// When I attempt to POST a new bill
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task CreateBill_Unauthenticated_Returns401()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var content = new StringContent("{\"name\":\"Test Bill\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/bills/accounts/{_testInstrumentId}/bills", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Authorization Denied Tests (403)

    /// <summary>
    /// Given I am authenticated but own a different account
    /// When I attempt to POST a bill for an account I don't own
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task CreateBill_NonOwner_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            AccountIds = [_otherInstrumentId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Test Bill\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/bills/accounts/{_testInstrumentId}/bills", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access (not owner) to the account
    /// When I attempt to POST a bill (which requires owner access)
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task CreateBill_SharedUser_Returns403()
    {
        // Arrange
        var user = new TestUser
        {
            FamilyId = _testFamilyId,
            SharedAccountIds = [_testInstrumentId],
        };
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Test Bill\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/bills/accounts/{_testInstrumentId}/bills", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Authorization Allowed Tests

    /// <summary>
    /// Given I am authenticated as the account owner
    /// When I request POST to create a bill for my account
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task CreateBill_Owner_PassesAuth()
    {
        // Arrange
        var user = TestUser.WithAccount(_testInstrumentId, _testFamilyId);
        var client = _factory.CreateAuthenticatedClient(user);

        // Act
        var content = new StringContent("{\"name\":\"Test Bill\"}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/api/bills/accounts/{_testInstrumentId}/bills", content, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
