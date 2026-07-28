#nullable enable
using System.Net.Http.Json;
using System.Text.Json;
using Asm.MooBank.Api.Tests.Authorization;
using Asm.MooBank.Api.Tests.Infrastructure;

namespace Asm.MooBank.Api.Tests.Retirement;

/// <summary>
/// End-to-end tests for editing a retirement plan through the API.
/// </summary>
/// <remarks>
/// Member reconciliation mutates a loaded graph — adding, updating and removing children — which
/// unit tests over a mocked repository cannot exercise, because nothing there tracks entity state
/// or writes to a store. These go through the real pipeline against a real change tracker.
/// </remarks>
[Collection(AuthorizationTestCollection.Name)]
[Trait("Category", "Integration")]
public class RetirementPlanUpdateTests(MooBankWebApplicationFactory factory)
{
    private readonly MooBankWebApplicationFactory _factory = factory;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static HttpClient CreateClient(MooBankWebApplicationFactory factory) =>
        factory.CreateAuthenticatedClient(new TestUser { FamilyId = Guid.NewGuid() });

    private static object Plan(params object[] members) => new
    {
        name = "Retirement",
        expectedReturnRate = 0.065m,
        inflationRate = 0.025m,
        superGuaranteeRate = 0.12m,
        contributionsTaxRate = 0.15m,
        lifeExpectancy = 90,
        members,
    };

    private static object Member(string name, Guid? id = null, IEnumerable<Guid>? instrumentIds = null) => new
    {
        id,
        name,
        currentAge = 45,
        salarySacrifice = 0m,
        growthStrategy = "Balanced",
        currentIncome = 100_000m,
        retirementAge = 65,
        instrumentIds = instrumentIds ?? [],
    };

    private static async Task<JsonElement> CreatePlanAsync(HttpClient client, params object[] members)
    {
        var response = await client.PostAsJsonAsync("/api/retirement/plans", Plan(members), TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, TestContext.Current.CancellationToken);
    }

    private static Guid IdOf(JsonElement plan) => plan.GetProperty("id").GetGuid();

    private static IEnumerable<JsonElement> MembersOf(JsonElement plan) => plan.GetProperty("members").EnumerateArray();

    /// <summary>
    /// Given a plan with an existing member
    /// When another person is added and the plan saved
    /// Then both members should be persisted
    /// </summary>
    [Fact]
    public async Task Update_AddingAPersonToAnExistingPlan_Succeeds()
    {
        // Arrange
        var client = CreateClient(_factory);
        var created = await CreatePlanAsync(client, Member("Self"));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId), Member("Spouse")),
            TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(["Self", "Spouse"], MembersOf(updated).Select(m => m.GetProperty("name").GetString()));
    }

    /// <summary>
    /// Given a plan with an existing member
    /// When a person is added
    /// Then the new member should survive a re-read of the plan
    /// </summary>
    [Fact]
    public async Task Update_AddingAPerson_PersistsToTheStore()
    {
        // Arrange
        var client = CreateClient(_factory);
        var created = await CreatePlanAsync(client, Member("Self"));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var update = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId), Member("Spouse")),
            TestContext.Current.CancellationToken);
        update.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(2, MembersOf(reread).Count());
    }

    /// <summary>
    /// Given a plan whose member has no accounts
    /// When the same member is saved again unchanged
    /// Then the save should succeed
    /// </summary>
    /// <remarks>
    /// Reconciliation rewrites a member's account links on every save, so an unchanged save still
    /// exercises the remove-and-recreate path.
    /// </remarks>
    [Fact]
    public async Task Update_SavingAnUnchangedMember_Succeeds()
    {
        // Arrange
        var client = CreateClient(_factory);
        var created = await CreatePlanAsync(client, Member("Self"));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId)),
            TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Given a plan with an existing member
    /// When a person with superannuation accounts is added
    /// Then those account links should be persisted
    /// </summary>
    /// <remarks>
    /// Account links are store-generated keys created through the same path as members, so they
    /// carry the same constraint: constructing one with an id already set makes EF write it as an
    /// update of a row that does not exist.
    /// </remarks>
    [Fact]
    public async Task Update_AddingAPersonWithAccounts_PersistsTheAccountLinks()
    {
        // Arrange
        var client = CreateClient(_factory);
        var created = await CreatePlanAsync(client, Member("Self"));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();
        var instrumentId = Guid.NewGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId), Member("Spouse", instrumentIds: [instrumentId])),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        var spouse = MembersOf(reread).Single(m => m.GetProperty("name").GetString() == "Spouse");
        Assert.Equal([instrumentId], spouse.GetProperty("instrumentIds").EnumerateArray().Select(i => i.GetGuid()));
    }

    /// <summary>
    /// Given a member with superannuation accounts
    /// When their set of accounts is changed
    /// Then the new set should replace the old one
    /// </summary>
    /// <remarks>
    /// Replacement is a clear-and-recreate, so this covers the old links actually being deleted
    /// rather than orphaned alongside the new ones. It also pins the editing specification to the
    /// link rows: including the required <c>Instrument</c> navigation joins the links away when no
    /// instrument matches, leaving nothing to clear.
    /// </remarks>
    [Fact]
    public async Task Update_ChangingAMembersAccounts_ReplacesTheSet()
    {
        // Arrange
        var client = CreateClient(_factory);
        var original = Guid.NewGuid();
        var replacement = Guid.NewGuid();

        var created = await CreatePlanAsync(client, Member("Self", instrumentIds: [original]));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId, instrumentIds: [replacement])),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        var self = MembersOf(reread).Single();
        Assert.Equal([replacement], self.GetProperty("instrumentIds").EnumerateArray().Select(i => i.GetGuid()));
    }

    /// <summary>
    /// Given a plan with two members
    /// When one is left out of the update
    /// Then only the remaining member should survive
    /// </summary>
    [Fact]
    public async Task Update_RemovingAPerson_DeletesThatMember()
    {
        // Arrange
        var client = CreateClient(_factory);
        var created = await CreatePlanAsync(client, Member("Self"), Member("Spouse"));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single(m => m.GetProperty("name").GetString() == "Self").GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member("Self", selfId)),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal("Self", MembersOf(reread).Single().GetProperty("name").GetString());
    }
}
