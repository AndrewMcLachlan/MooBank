#nullable enable
using System.Net.Http.Json;
using System.Text.Json;
using Asm.MooBank.Api.Tests.Authorization;
using Asm.MooBank.Api.Tests.Infrastructure;
using Asm.MooBank.Domain.Entities.Instrument;

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

    /// <summary>
    /// A caller plus two people in their family, and an instrument owned by each.
    /// </summary>
    /// <remarks>
    /// A member now names a user and can only hold accounts that user owns, so both have to exist
    /// before the API will accept a plan — the guard rejects anything else.
    /// </remarks>
    private sealed record Household(HttpClient Client, Guid SelfUserId, Guid SpouseUserId, Guid SelfInstrumentId, Guid SelfOtherInstrumentId, Guid SpouseInstrumentId);

    private async Task<Household> CreateHouseholdAsync()
    {
        var familyId = Guid.NewGuid();
        var self = Guid.NewGuid();
        var spouse = Guid.NewGuid();
        var selfInstrument = Guid.NewGuid();
        var selfOtherInstrument = Guid.NewGuid();
        var spouseInstrument = Guid.NewGuid();

        await _factory.SeedDataAsync(async context =>
        {
            context.Add(new Asm.MooBank.Domain.Entities.User.User(self) { EmailAddress = $"self-{self}@example.com", FirstName = "Self", FamilyId = familyId });
            context.Add(new Asm.MooBank.Domain.Entities.User.User(spouse) { EmailAddress = $"spouse-{spouse}@example.com", FirstName = "Spouse", FamilyId = familyId });

            // Ownership rows only: the in-memory provider does not enforce the foreign key, and the
            // guard reads ownership rather than the instrument itself.
            context.Add(new InstrumentOwner { UserId = self, InstrumentId = selfInstrument });
            context.Add(new InstrumentOwner { UserId = self, InstrumentId = selfOtherInstrument });
            context.Add(new InstrumentOwner { UserId = spouse, InstrumentId = spouseInstrument });

            await context.SaveChangesAsync();
        });

        var client = _factory.CreateAuthenticatedClient(new TestUser { Id = self, FamilyId = familyId });

        return new Household(client, self, spouse, selfInstrument, selfOtherInstrument, spouseInstrument);
    }

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

    private static object Member(Guid userId, Guid? id = null, IEnumerable<Guid>? instrumentIds = null) => new
    {
        id,
        userId,
        currentAge = 45,
        salarySacrifice = 0m,
        growthStrategy = "Balanced",
        annualFees = 0m,
        insurancePremium = 0m,
        currentIncome = 100_000m,
        retirementAge = 65,
        instrumentIds = instrumentIds ?? [],
    };

    private static async Task<JsonElement> CreatePlanAsync(HttpClient client, params object[] members)
    {
        var response = await client.PostAsJsonAsync("/api/retirement/plans", Plan(members), TestContext.Current.CancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // The body carries the validation or authorisation reason, which a bare status code hides.
            throw new Exception($"Create failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)}");
        }

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
        var h = await CreateHouseholdAsync();
        var client = h.Client;
        var created = await CreatePlanAsync(client, Member(h.SelfUserId));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId), Member(h.SpouseUserId)),
            TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal([h.SelfUserId, h.SpouseUserId], MembersOf(updated).Select(m => m.GetProperty("userId").GetGuid()));
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
        var h = await CreateHouseholdAsync();
        var client = h.Client;
        var created = await CreatePlanAsync(client, Member(h.SelfUserId));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var update = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId), Member(h.SpouseUserId)),
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
        var h = await CreateHouseholdAsync();
        var client = h.Client;
        var created = await CreatePlanAsync(client, Member(h.SelfUserId));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId)),
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
        var h = await CreateHouseholdAsync();
        var client = h.Client;
        var created = await CreatePlanAsync(client, Member(h.SelfUserId));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();


        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId), Member(h.SpouseUserId, instrumentIds: [h.SpouseInstrumentId])),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        var spouse = MembersOf(reread).Single(m => m.GetProperty("userId").GetGuid() == h.SpouseUserId);
        Assert.Equal([h.SpouseInstrumentId], spouse.GetProperty("instrumentIds").EnumerateArray().Select(i => i.GetGuid()));
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
        var h = await CreateHouseholdAsync();
        var client = h.Client;



        var created = await CreatePlanAsync(client, Member(h.SelfUserId, instrumentIds: [h.SelfInstrumentId]));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single().GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId, instrumentIds: [h.SelfOtherInstrumentId])),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        var self = MembersOf(reread).Single();
        Assert.Equal([h.SelfOtherInstrumentId], self.GetProperty("instrumentIds").EnumerateArray().Select(i => i.GetGuid()));
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
        var h = await CreateHouseholdAsync();
        var client = h.Client;
        var created = await CreatePlanAsync(client, Member(h.SelfUserId), Member(h.SpouseUserId));
        var planId = IdOf(created);
        var selfId = MembersOf(created).Single(m => m.GetProperty("userId").GetGuid() == h.SelfUserId).GetProperty("id").GetGuid();

        // Act
        var response = await client.PutAsJsonAsync(
            $"/api/retirement/plans/{planId}",
            Plan(Member(h.SelfUserId, selfId)),
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/retirement/plans/{planId}", JsonOptions, TestContext.Current.CancellationToken);
        Assert.Equal(h.SelfUserId, MembersOf(reread).Single().GetProperty("userId").GetGuid());
    }
}
