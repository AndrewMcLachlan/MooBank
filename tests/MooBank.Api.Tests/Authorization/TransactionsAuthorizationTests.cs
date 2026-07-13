#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for transaction authorization. The whole Transactions endpoint group is protected
/// by GetInstrumentViewerPolicy (viewers may read/tag transactions), resolved from cached claims.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class TransactionsAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;
    private readonly Guid _instrumentId = Guid.NewGuid();

    private string TransactionsUrl => $"/api/accounts/{_instrumentId}/transactions";

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for an account's transactions
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task GetTransactions_Unauthenticated_Returns401()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(TransactionsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but do not have access to the account
    /// When I request GET for its transactions
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GetTransactions_NonViewer_Returns403()
    {
        var user = new TestUser { AccountIds = [Guid.NewGuid()] };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(TransactionsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as an owner of the account
    /// When I request GET for its transactions
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GetTransactions_Owner_PassesAuth()
    {
        var user = TestUser.WithAccount(_instrumentId);
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(TransactionsUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated with shared access to the account
    /// When I request GET for its transactions
    /// Then authorization should pass (viewers include shared access)
    /// </summary>
    [Fact]
    public async Task GetTransactions_SharedViewer_PassesAuth()
    {
        var user = TestUser.WithSharedAccount(_instrumentId);
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(TransactionsUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
