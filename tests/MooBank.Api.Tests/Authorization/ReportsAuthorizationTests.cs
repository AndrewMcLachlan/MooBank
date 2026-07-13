#nullable enable
using Asm.MooBank.Api.Tests.Infrastructure;

namespace Asm.MooBank.Api.Tests.Authorization;

/// <summary>
/// Integration tests for report authorization. Account reports are protected by GetInstrumentViewerPolicy
/// (accountId) and group reports by GetGroupOwnerPolicy (groupId), both resolved from cached claims.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class ReportsAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Guid _groupId = Guid.NewGuid();

    private string AccountReportUrl => $"/api/accounts/{_accountId}/reports/in-out/2026-01-01/2026-12-31";
    private string GroupReportUrl => $"/api/groups/{_groupId}/reports/monthly-balances/2026-01-01/2026-12-31";

    #region Account reports (InstrumentViewer)

    /// <summary>
    /// Given I am not authenticated
    /// When I request an account report
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    public async Task AccountReport_Unauthenticated_Returns401()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(AccountReportUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but do not have access to the account
    /// When I request an account report
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task AccountReport_NonViewer_Returns403()
    {
        var user = new TestUser { AccountIds = [Guid.NewGuid()] };
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(AccountReportUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as an owner of the account
    /// When I request an account report
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task AccountReport_Owner_PassesAuth()
    {
        var user = TestUser.WithAccount(_accountId);
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(AccountReportUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Group reports (GroupOwner)

    /// <summary>
    /// Given I am authenticated but own a different group
    /// When I request a group report
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    public async Task GroupReport_NonOwner_Returns403()
    {
        var user = TestUser.WithGroup(Guid.NewGuid());
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(GroupReportUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as the group owner
    /// When I request a group report
    /// Then authorization should pass
    /// </summary>
    [Fact]
    public async Task GroupReport_Owner_PassesAuth()
    {
        var user = TestUser.WithGroup(_groupId);
        var client = _factory.CreateAuthenticatedClient(user);

        var response = await client.GetAsync(GroupReportUrl, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
