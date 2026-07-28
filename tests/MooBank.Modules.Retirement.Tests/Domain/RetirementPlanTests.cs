#nullable enable
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Domain;

/// <summary>
/// Unit tests for the retirement plan aggregate.
/// </summary>
[Trait("Category", "Unit")]
public class RetirementPlanTests
{
    private static readonly RetirementAssumptions Assumptions = new(0.065m, 0.025m, 0.12m, 0.15m, 90);

    /// <summary>
    /// Given a family and a set of assumptions
    /// When a plan is created
    /// Then the assumptions should be applied and the timestamps set
    /// </summary>
    [Fact]
    public void Create_NewPlan_AppliesAssumptions()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        // Act
        var plan = RetirementPlan.Create(familyId, "Retirement", Assumptions);

        // Assert
        Assert.Equal(familyId, plan.FamilyId);
        Assert.Equal("Retirement", plan.Name);
        Assert.Equal(0.065m, plan.ExpectedReturnRate);
        Assert.Equal(0.025m, plan.InflationRate);
        Assert.Equal(0.12m, plan.SuperGuaranteeRate);
        Assert.Equal(0.15m, plan.ContributionsTaxRate);
        Assert.Equal(90, plan.LifeExpectancy);
        Assert.NotEqual(default, plan.CreatedUtc);
        Assert.NotEqual(Guid.Empty, plan.Id);
    }

    /// <summary>
    /// Given a plan
    /// When a member is added
    /// Then the member should be linked to the plan with their instruments
    /// </summary>
    [Fact]
    public void AddMember_NewMember_IsLinkedToThePlan()
    {
        // Arrange
        var plan = RetirementPlan.Create(Guid.NewGuid(), "Retirement", Assumptions);
        var instrumentId = Guid.NewGuid();

        // Act
        var member = plan.AddMember("Self", new DateOnly(1980, 1, 1), 100_000m, 65, [instrumentId]);

        // Assert
        Assert.Equal(plan.Id, member.RetirementPlanId);
        Assert.Equal(member, Assert.Single(plan.Members));
        Assert.Equal(instrumentId, Assert.Single(member.Accounts).InstrumentId);
    }

    /// <summary>
    /// Given a member of the plan
    /// When they are removed
    /// Then the plan should no longer hold them
    /// </summary>
    [Fact]
    public void RemoveMember_ExistingMember_IsRemoved()
    {
        // Arrange
        var plan = RetirementPlan.Create(Guid.NewGuid(), "Retirement", Assumptions);
        var member = plan.AddMember("Self", new DateOnly(1980, 1, 1), 100_000m, 65, []);

        // Act
        plan.RemoveMember(member.Id);

        // Assert
        Assert.Empty(plan.Members);
    }

    /// <summary>
    /// Given an id that is not a member of the plan
    /// When it is removed
    /// Then it should fail rather than silently do nothing
    /// </summary>
    [Fact]
    public void RemoveMember_UnknownMember_Throws()
    {
        // Arrange
        var plan = RetirementPlan.Create(Guid.NewGuid(), "Retirement", Assumptions);

        // Act / Assert
        Assert.Throws<NotFoundException>(() => plan.RemoveMember(Guid.NewGuid()));
    }

    /// <summary>
    /// Given a plan
    /// When it is updated
    /// Then the new name and assumptions should be applied
    /// </summary>
    [Fact]
    public void Update_NewAssumptions_AreApplied()
    {
        // Arrange
        var plan = RetirementPlan.Create(Guid.NewGuid(), "Retirement", Assumptions);

        // Act
        plan.Update("Renamed", new RetirementAssumptions(0.08m, 0.03m, 0.11m, 0.15m, 95));

        // Assert
        Assert.Equal("Renamed", plan.Name);
        Assert.Equal(0.08m, plan.ExpectedReturnRate);
        Assert.Equal(95, plan.LifeExpectancy);
    }

    /// <summary>
    /// Given a member whose accounts are replaced
    /// When the new set is applied
    /// Then only the new instruments should remain
    /// </summary>
    [Fact]
    public void SetAccounts_ReplacingTheSet_KeepsOnlyTheNewInstruments()
    {
        // Arrange
        var member = TestEntities.CreateMember(accountBalances: [1m, 2m]);
        var replacement = Guid.NewGuid();

        // Act
        member.SetAccounts([replacement]);

        // Assert
        Assert.Equal(replacement, Assert.Single(member.Accounts).InstrumentId);
    }

    /// <summary>
    /// Given a date of birth
    /// When the age is taken at a date
    /// Then a birthday that has not been reached should not be counted
    /// </summary>
    [Theory]
    // Birthday already passed this year.
    [InlineData("1980-01-01", "2026-06-01", 46)]
    // Birthday falls exactly on the date.
    [InlineData("1980-06-01", "2026-06-01", 46)]
    // Birthday still to come this year.
    [InlineData("1980-12-31", "2026-06-01", 45)]
    // The day before their birthday.
    [InlineData("1980-06-02", "2026-06-01", 45)]
    public void AgeAt_ByBirthday_CountsOnlyCompletedYears(string dateOfBirth, string at, int expected)
    {
        // Arrange
        var member = TestEntities.CreateMember(dateOfBirth: DateOnly.Parse(dateOfBirth));

        // Act
        var age = member.AgeAt(DateOnly.Parse(at));

        // Assert
        Assert.Equal(expected, age);
    }
}
