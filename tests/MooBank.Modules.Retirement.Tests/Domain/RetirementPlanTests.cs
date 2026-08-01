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
    private static readonly RetirementAssumptions Assumptions = new(0.065m, 0.025m, 0.12m, 0.15m, 90, 60_000m, 2, 0.03m);

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
        var member = plan.AddMember(Guid.NewGuid(), 45, 100_000m, 0m, 65, GrowthStrategy.Balanced, 0m, 0m, [instrumentId]);

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
        var member = plan.AddMember(Guid.NewGuid(), 45, 100_000m, 0m, 65, GrowthStrategy.Balanced, 0m, 0m, []);

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
        plan.Update("Renamed", new RetirementAssumptions(0.08m, 0.03m, 0.11m, 0.15m, 95, 70_000m, 5, 0.035m));

        // Assert
        Assert.Equal("Renamed", plan.Name);
        Assert.Equal(0.08m, plan.ExpectedReturnRate);
        Assert.Equal(95, plan.LifeExpectancy);
        Assert.Equal(70_000m, plan.TargetRetirementIncome);
        Assert.Equal(5, plan.CashBucketYears);
        Assert.Equal(0.035m, plan.CashReturnRate);
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
    /// Given a member
    /// When they are updated
    /// Then their income, salary sacrifice, age and strategy should all be applied
    /// </summary>
    [Fact]
    public void Update_Member_AppliesEveryField()
    {
        // Arrange
        var member = TestEntities.CreateMember(name: "Self", currentAge: 45, currentIncome: 100_000m, salarySacrifice: 0m, retirementAge: 65, growthStrategy: GrowthStrategy.Balanced);

        // Act
        member.Update(50, 130_000m, 12_000m, 62, GrowthStrategy.Conservative, 250m, 300m);

        // Assert
        Assert.Equal(50, member.CurrentAge);
        Assert.Equal(130_000m, member.CurrentIncome);
        Assert.Equal(12_000m, member.SalarySacrifice);
        Assert.Equal(62, member.RetirementAge);
        Assert.Equal(GrowthStrategy.Conservative, member.GrowthStrategy);
    }
}
