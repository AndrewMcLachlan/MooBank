#nullable enable
using System.Net;
using System.Net.Http.Json;
using Asm.MooBank.Api.Tests.Authorization;
using Asm.MooBank.Api.Tests.Infrastructure;

namespace Asm.MooBank.Api.Tests.ReferenceData;

/// <summary>
/// Tests that editing the Age Pension rates is restricted while reading them is not.
/// </summary>
/// <remarks>
/// These rates are national, so one family changing them would change every other family's
/// projection. That asymmetry — open to read, restricted to write — is the whole reason the endpoint
/// carries a policy, so it is worth a test rather than trusting the attribute is still there.
/// </remarks>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class PensionRatesAuthorizationTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    private static object Rates() => new
    {
        rates = new
        {
            id = 0,
            effectiveFrom = "2026-03-20",
            eligibilityAge = 67,
            maxAnnualSingle = 30_000m,
            maxAnnualCouple = 45_000m,
            assetsFreeAreaSingle = 320_000m,
            assetsFreeAreaCouple = 480_000m,
            assetsTaperRate = 0.078m,
        },
    };

    /// <summary>
    /// Given an ordinary signed-in user
    /// When they read the pension rates
    /// Then they should be allowed
    /// </summary>
    [Fact]
    public async Task Get_OrdinaryUser_IsAllowed()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(new TestUser());

        // Act
        var response = await client.GetAsync("/api/reference-data/pension-rates", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Given an ordinary signed-in user
    /// When they try to change the pension rates
    /// Then they should be refused
    /// </summary>
    [Fact]
    public async Task Save_OrdinaryUser_IsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(new TestUser());

        // Act
        var response = await client.PutAsJsonAsync("/api/reference-data/pension-rates", Rates(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given nobody signed in
    /// When the pension rates are read
    /// Then it should be refused
    /// </summary>
    [Fact]
    public async Task Get_Anonymous_IsUnauthorised()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/reference-data/pension-rates", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
