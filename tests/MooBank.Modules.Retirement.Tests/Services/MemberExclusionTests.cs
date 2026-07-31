#nullable enable
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for leaving a member out of a projection, so a household can be seen one person at a
/// time.
/// </summary>
[Trait("Category", "Unit")]
public class MemberExclusionTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private static readonly AgePensionRates Rates = new(67, 29_900m, 45_080m, 314_000m, 470_000m, 0.078m);

    private readonly RetirementProjectionEngine _engine = new();

    private static (Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan, Guid SelfId, Guid SpouseId) Household()
    {
        var self = TestEntities.CreateMember(name: "Self", currentAge: 60, retirementAge: 65, currentIncome: 100_000m, accountBalances: [400_000m]);
        var spouse = TestEntities.CreateMember(name: "Spouse", currentAge: 60, retirementAge: 65, currentIncome: 80_000m, accountBalances: [200_000m]);

        return (TestEntities.CreatePlan(targetRetirementIncome: 50_000m, members: [self, spouse]), self.Id, spouse.Id);
    }

    /// <summary>
    /// Given a household of two
    /// When one is excluded
    /// Then only the other should be projected
    /// </summary>
    [Fact]
    public void Calculate_AnExcludedMember_IsLeftOut()
    {
        // Arrange
        var (plan, selfId, spouseId) = Household();

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today, new ProjectionOverrides { ExcludedMemberIds = [spouseId] });

        // Assert
        var member = Assert.Single(projection.Members);
        Assert.Equal(selfId, member.MemberId);

        // Their balance is gone from the household too, not merely hidden from the list.
        Assert.Equal(400_000m, projection.Summary.CurrentBalance);
    }

    /// <summary>
    /// Given a member excluded from a projection
    /// When the exclusion is dropped
    /// Then they should be projected again
    /// </summary>
    /// <remarks>
    /// The exclusion is a view of the plan, not an edit to it.
    /// </remarks>
    [Fact]
    public void Calculate_WithoutTheExclusion_TheyAreBackAgain()
    {
        // Arrange
        var (plan, _, spouseId) = Household();

        // Act
        var without = _engine.CalculateWithoutPension(plan, Today, new ProjectionOverrides { ExcludedMemberIds = [spouseId] });
        var with = _engine.CalculateWithoutPension(plan, Today);

        // Assert
        Assert.Single(without.Members);
        Assert.Equal(2, with.Members.Count());
        Assert.Equal(600_000m, with.Summary.CurrentBalance);
    }

    /// <summary>
    /// Given every member excluded
    /// When the projection is run
    /// Then it should project the whole household rather than nothing
    /// </summary>
    /// <remarks>
    /// A projection of nobody answers no question, and an empty result reads as a broken plan rather
    /// than an empty filter.
    /// </remarks>
    [Fact]
    public void Calculate_EveryoneExcluded_ProjectsThemAll()
    {
        // Arrange
        var (plan, selfId, spouseId) = Household();

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today, new ProjectionOverrides { ExcludedMemberIds = [selfId, spouseId] });

        // Assert
        Assert.Equal(2, projection.Members.Count());
    }

    /// <summary>
    /// Given a member id that is not on the plan
    /// When it is excluded
    /// Then the projection should be unaffected
    /// </summary>
    [Fact]
    public void Calculate_ExcludingSomeoneNotOnThePlan_ChangesNothing()
    {
        // Arrange
        var (plan, _, _) = Household();

        // Act
        var projection = _engine.CalculateWithoutPension(plan, Today, new ProjectionOverrides { ExcludedMemberIds = [Guid.NewGuid()] });

        // Assert
        Assert.Equal(2, projection.Members.Count());
    }

    /// <summary>
    /// Given a couple where one is excluded
    /// When the Age Pension is worked out
    /// Then the remaining person should be assessed as a single
    /// </summary>
    /// <remarks>
    /// What makes the answer a genuine "just me" rather than half of a couple's. The single rate and
    /// the single free area are both lower, so the pension differs by more than a halving.
    /// </remarks>
    [Fact]
    public void Calculate_OneOfACoupleExcluded_IsAssessedAsSingle()
    {
        // Arrange: retired, and inside both free areas so the full rate applies either way.
        var self = TestEntities.CreateMember(name: "Self", currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [150_000m]);
        var spouse = TestEntities.CreateMember(name: "Spouse", currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [100_000m]);
        var plan = TestEntities.CreatePlan(inflationRate: 0m, cashReturnRate: 0m, targetRetirementIncome: 60_000m, members: [self, spouse]);

        // Act
        var asCouple = _engine.Calculate(plan, Today, Rates).Years.ElementAt(1);
        var asSingle = _engine.Calculate(plan, Today, Rates, new ProjectionOverrides { ExcludedMemberIds = [spouse.Id] }).Years.ElementAt(1);

        // Assert
        Assert.Equal(45_080m, asCouple.Pension);
        Assert.Equal(29_900m, asSingle.Pension);
    }
}
