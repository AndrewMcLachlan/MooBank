#nullable enable
using Asm.MooBank.Api.Tests.Authorization;
using Asm.MooBank.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Api.Tests.Errors;

/// <summary>
/// A request whose parameters cannot be bound is answered with problem details naming the
/// parameter, in every environment.
/// </summary>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class BindingErrorTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    /// <summary>
    /// Given I can view an account
    /// When I request its transactions with a query value that cannot be bound
    /// Then the response is a 400 whose problem details name the parameter
    /// </summary>
    [Fact]
    public async Task GetTransactions_UnbindableQueryValue_ReturnsProblemDetails()
    {
        var instrumentId = Guid.NewGuid();
        var client = _factory.CreateAuthenticatedClient(TestUser.WithAccount(instrumentId));

        var response = await client.GetAsync($"/api/accounts/{instrumentId}/transactions/50/1?SortDirection=Descending&TransactionType=Sideways", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        Assert.Contains("TransactionType", problem?.Detail);
    }
}
