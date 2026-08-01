#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the sustainable income solved against the projection itself.
/// </summary>
/// <remarks>
/// This is the figure a target income is set from, so what matters is not that it is close to some
/// formula but that the plan can actually pay it — and cannot pay meaningfully more.
/// </remarks>
[Trait("Category", "Unit")]
public class SustainableSolveTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private static readonly AgePensionRates Rates = new(67, 29_900m, 45_080m, 314_000m, 470_000m, 0.078m);

    private readonly RetirementProjectionEngine _engine = new();

    private static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan(decimal target = 0m, int lifeExpectancy = 85) =>
        TestEntities.CreatePlan(
            expectedReturnRate: 0.065m, inflationRate: 0.025m, superGuaranteeRate: 0.12m,
            contributionsTaxRate: 0.15m, lifeExpectancy: lifeExpectancy,
            targetRetirementIncome: target, preRetirementSwitchYears: 2, cashReturnRate: 0.03m,
            members: [
                TestEntities.CreateMember(name: "Andy", currentAge: 55, retirementAge: 67, currentIncome: 150_000m,
                    annualFees: 400m, insurancePremium: 300m, accountBalances: [600_000m]),
            ]);

    /// <summary>
    /// Given the solved income
    /// When the plan is run targeting it
    /// Then it should last, and a materially larger target should not
    /// </summary>
    /// <remarks>
    /// The property that makes the figure worth trusting, checked from both sides. An annuity on the
    /// closing balance fails the second half badly: it knows nothing of the fees still being charged
    /// or the pension arriving, and reads high.
    /// </remarks>
    [Fact]
    public void Solve_TheSustainableIncome_LastsAndIsNearlyTheMost()
    {
        // Act
        var solved = _engine.Calculate(Plan(), Today, Rates).Summary.SustainableIncomeInTodaysDollars;

        // Assert
        Assert.True(solved > 0m);

        var atSolved = _engine.Calculate(Plan(solved), Today, Rates).Summary;
        Assert.Null(atSolved.MoneyRunsOutYear);

        // A hundred pounds more than the solve claims is beyond the rounding it allows for.
        var justOver = _engine.Calculate(Plan(solved + 500m), Today, Rates).Summary;
        Assert.NotNull(justOver.MoneyRunsOutYear);
    }

    /// <summary>
    /// Given a plan solved with the Age Pension and without it
    /// When the two are compared
    /// Then the pension should allow a larger income
    /// </summary>
    /// <remarks>
    /// Why the annuity was wrong to leave it out: the pension is not a rounding difference.
    /// </remarks>
    [Fact]
    public void Solve_WithThePension_AllowsMoreThanWithout()
    {
        var withPension = _engine.Calculate(Plan(), Today, Rates).Summary.SustainableIncomeInTodaysDollars;
        var without = _engine.Calculate(Plan(), Today, AgePensionRates.None).Summary.SustainableIncomeInTodaysDollars;

        Assert.True(withPension > without, $"expected the pension to allow more, but got {withPension} against {without}");
    }

    /// <summary>
    /// Given a longer retirement
    /// When the income is solved
    /// Then it should be smaller
    /// </summary>
    [Fact]
    public void Solve_ALongerRetirement_SustainsLess()
    {
        var to85 = _engine.Calculate(Plan(lifeExpectancy: 85), Today, Rates).Summary.SustainableIncomeInTodaysDollars;
        var to95 = _engine.Calculate(Plan(lifeExpectancy: 95), Today, Rates).Summary.SustainableIncomeInTodaysDollars;

        Assert.True(to95 < to85, $"expected a longer retirement to sustain less, but got {to95} against {to85}");
    }

    /// <summary>
    /// Given a household with one member excluded
    /// When the income is solved
    /// Then it should be solved for the people who remain
    /// </summary>
    /// <remarks>
    /// The solve has to follow the exclusion, or setting a target from it would hand one person a
    /// couple's income.
    /// </remarks>
    [Fact]
    public void Solve_WithAMemberExcluded_IsForTheRemainingHousehold()
    {
        // Arrange
        var self = TestEntities.CreateMember(name: "Self", currentAge: 60, retirementAge: 67, currentIncome: 100_000m, accountBalances: [500_000m]);
        var spouse = TestEntities.CreateMember(name: "Spouse", currentAge: 60, retirementAge: 67, currentIncome: 100_000m, accountBalances: [500_000m]);
        var plan = TestEntities.CreatePlan(lifeExpectancy: 85, members: [self, spouse]);

        // Act
        var household = _engine.Calculate(plan, Today, Rates).Summary.SustainableIncomeInTodaysDollars;
        var alone = _engine.Calculate(plan, Today, Rates, new ProjectionOverrides { ExcludedMemberIds = [spouse.Id] })
            .Summary.SustainableIncomeInTodaysDollars;

        // Assert
        Assert.True(alone < household, $"one person should sustain less than two, but got {alone} against {household}");
        Assert.True(alone > 0m);
    }

    /// <summary>
    /// Given a plan with no members
    /// When the income is solved
    /// Then it should be nought rather than fail
    /// </summary>
    [Fact]
    public void Solve_NoMembers_IsNought()
    {
        var plan = TestEntities.CreatePlan(members: []);

        Assert.Equal(0m, _engine.Calculate(plan, Today, Rates).Summary.SustainableIncomeInTodaysDollars);
    }
}
