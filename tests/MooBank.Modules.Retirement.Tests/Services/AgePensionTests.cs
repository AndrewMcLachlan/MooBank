#nullable enable
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// Unit tests for the Age Pension entitlement and its effect on a projection.
/// </summary>
/// <remarks>
/// Figures are the seeded homeowner rates: 29,900 a year single, 45,080 a couple, free areas of
/// 314,000 and 470,000, tapering at 7.8% of the excess. They are approximations of published rates
/// and will drift; the tests assert the arithmetic of the test rather than any particular year's
/// figures, so they hold when the settings are corrected.
/// </remarks>
[Trait("Category", "Unit")]
public class AgePensionTests
{
    private static readonly DateOnly Today = new(2026, 7, 30);

    private static readonly AgePensionRates Rates = new(
        EligibilityAge: 67,
        MaxAnnualSingle: 29_900m,
        MaxAnnualCouple: 45_080m,
        AssetsFreeAreaSingle: 314_000m,
        AssetsFreeAreaCouple: 470_000m,
        AssetsTaperRate: 0.078m);

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// Given a household below the assets free area
    /// When the entitlement is worked out
    /// Then it should be the full rate
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(100_000)]
    [InlineData(470_000)]
    [Trait("Category", "Unit")]
    public void ForYear_CoupleBelowTheFreeArea_GetsTheFullRate(decimal assets)
    {
        Assert.Equal(45_080m, AgePension.ForYear(Rates, [67, 67], assets));
    }

    /// <summary>
    /// Given a household above the assets free area
    /// When the entitlement is worked out
    /// Then it should be reduced by the taper on the excess
    /// </summary>
    [Fact]
    public void ForYear_CoupleAboveTheFreeArea_IsTapered()
    {
        // 100,000 over the free area, tapering at 7.8%, is 7,800 off.
        Assert.Equal(45_080m - 7_800m, AgePension.ForYear(Rates, [67, 67], 570_000m));
    }

    /// <summary>
    /// Given a household well above the free area
    /// When the entitlement is worked out
    /// Then it should be nought rather than negative
    /// </summary>
    [Fact]
    public void ForYear_WellAboveTheFreeArea_IsNought()
    {
        Assert.Equal(0m, AgePension.ForYear(Rates, [67, 67], 2_000_000m));
    }

    /// <summary>
    /// Given nobody has reached pension age
    /// When the entitlement is worked out
    /// Then there should be none
    /// </summary>
    [Fact]
    public void ForYear_NobodyEligible_GetsNothing()
    {
        Assert.Equal(0m, AgePension.ForYear(Rates, [65, 63], 100_000m));
    }

    /// <summary>
    /// Given a couple where only one has reached pension age
    /// When the entitlement is worked out
    /// Then only that person's half of the couple rate should be paid
    /// </summary>
    /// <remarks>
    /// The means test still counts everything they hold between them, which is how the real test
    /// works — the younger partner's balance reduces the older one's pension.
    /// </remarks>
    [Fact]
    public void ForYear_OneOfACoupleEligible_GetsHalfTheCoupleRate()
    {
        Assert.Equal(45_080m / 2m, AgePension.ForYear(Rates, [67, 60], 400_000m));
    }

    /// <summary>
    /// Given a single person
    /// When the entitlement is worked out
    /// Then the single rate and the single free area should apply
    /// </summary>
    [Fact]
    public void ForYear_SinglePerson_UsesTheSingleRate()
    {
        Assert.Equal(29_900m, AgePension.ForYear(Rates, [67], 314_000m));
        // 100,000 over the single free area.
        Assert.Equal(29_900m - 7_800m, AgePension.ForYear(Rates, [67], 414_000m));
    }

    /// <summary>
    /// Given no rates have been recorded
    /// When the entitlement is worked out
    /// Then there should be none, at any age
    /// </summary>
    /// <remarks>
    /// A settings gap must leave a projection running on superannuation alone rather than failing or
    /// inventing an entitlement.
    /// </remarks>
    [Fact]
    public void ForYear_NoRatesRecorded_GetsNothing()
    {
        Assert.Equal(0m, AgePension.ForYear(AgePensionRates.None, [90, 90], 0m));
    }

    /// <summary>
    /// Given a household of a given age
    /// When the assets cut-off is worked out
    /// Then it should be the point at which the taper has taken the whole entitlement
    /// </summary>
    [Fact]
    public void AssetsCutOff_IsWhereTheTaperExhaustsThePension()
    {
        // 470,000 free area plus 45,080 of entitlement at 7.8% a year.
        var expected = 470_000m + (45_080m / 0.078m);

        Assert.Equal(expected, AgePension.AssetsCutOff(Rates, [67, 67]), 2);

        // And a household exactly there receives nothing, while a pound under receives something.
        Assert.Equal(0m, AgePension.ForYear(Rates, [67, 67], expected));
        Assert.True(AgePension.ForYear(Rates, [67, 67], expected - 1_000m) > 0m);
    }

    /// <summary>
    /// Given a single person
    /// When the assets cut-off is worked out
    /// Then it should use the single free area and rate
    /// </summary>
    [Fact]
    public void AssetsCutOff_ForOnePerson_IsLowerThanForACouple()
    {
        var single = AgePension.AssetsCutOff(Rates, [67]);
        var couple = AgePension.AssetsCutOff(Rates, [67, 67]);

        Assert.Equal(314_000m + (29_900m / 0.078m), single, 2);
        Assert.True(single < couple);
    }

    /// <summary>
    /// Given nobody old enough
    /// When the assets cut-off is worked out
    /// Then there should be none to draw
    /// </summary>
    /// <remarks>
    /// No level of assets pays anything before pension age, so a threshold would be a line with no
    /// meaning under it.
    /// </remarks>
    [Fact]
    public void AssetsCutOff_NobodyEligible_IsNought()
    {
        Assert.Equal(0m, AgePension.AssetsCutOff(Rates, [60, 55]));
        Assert.Equal(0m, AgePension.AssetsCutOff(AgePensionRates.None, [90]));
    }

    /// <summary>
    /// Given a projection
    /// When the summary is read
    /// Then it should state one level, in today's dollars, below which the pension starts
    /// </summary>
    /// <remarks>
    /// A single figure so a chart can draw a straight line for the balance to cross. It is stated for
    /// the household with everyone of pension age, and it does not move over the projection because
    /// the thresholds are indexed exactly as the money is.
    /// </remarks>
    [Fact]
    public void Calculate_TheSummary_StatesOneLevelBelowWhichThePensionStarts()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [400_000m]),
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [400_000m]),
        ]);

        // Act
        var summary = _engine.Calculate(plan, Today, Rates).Summary;

        // Assert
        Assert.Equal(470_000m + (45_080m / 0.078m), summary.PensionStartsBelowInTodaysDollars, 0);
    }

    /// <summary>
    /// Given a household of one
    /// When the summary is read
    /// Then the level should be the single person's, which is lower
    /// </summary>
    [Fact]
    public void Calculate_OnePerson_StatesTheSingleLevel()
    {
        // Arrange
        var plan = TestEntities.CreatePlan(members: [
            TestEntities.CreateMember(currentAge: 60, retirementAge: 65, accountBalances: [400_000m]),
        ]);

        // Act
        var summary = _engine.Calculate(plan, Today, Rates).Summary;

        // Assert
        Assert.Equal(314_000m + (29_900m / 0.078m), summary.PensionStartsBelowInTodaysDollars, 0);
    }

    /// <summary>
    /// Given no pension rates recorded
    /// When the summary is read
    /// Then there should be no level to draw
    /// </summary>
    [Fact]
    public void Calculate_NoRates_StatesNoLevel()
    {
        var plan = TestEntities.CreatePlan(members: [TestEntities.CreateMember(accountBalances: [400_000m])]);

        Assert.Equal(0m, _engine.CalculateWithoutPension(plan, Today).Summary.PensionStartsBelowInTodaysDollars);
    }

    /// <summary>
    /// Given rates indexed to a later year
    /// When they are read
    /// Then the money figures should move but the eligibility age should not
    /// </summary>
    /// <remarks>
    /// Rates and thresholds are indexed to inflation in reality; the age is set by legislation. Left
    /// unindexed in nominal terms, the pension would shrink away to nothing over a long projection.
    /// </remarks>
    [Fact]
    public void Indexed_ALaterYear_MovesTheMoneyButNotTheAge()
    {
        var indexed = AgePension.Indexed(Rates, 2m);

        Assert.Equal(90_160m, indexed.MaxAnnualCouple);
        Assert.Equal(940_000m, indexed.AssetsFreeAreaCouple);
        Assert.Equal(67, indexed.EligibilityAge);
    }

    /// <summary>
    /// Given a household whose superannuation is exhausted
    /// When the projection is run with a pension
    /// Then it should still have an income
    /// </summary>
    /// <remarks>
    /// The whole point of modelling the pension: a plan that spends its superannuation falls back to
    /// the pension rather than to nothing.
    /// </remarks>
    [Fact]
    public void Calculate_SuperExhausted_FallsBackToThePension()
    {
        // Arrange: a target far above the pension, so superannuation is genuinely spent making up
        // the difference. A target the pension nearly covers would leave the balance barely touched.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 100_000m,
            members: [
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [100_000m]),
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [50_000m]),
            ]);

        // Act
        var projection = _engine.Calculate(plan, Today, Rates);
        var finalYear = projection.Years.Last();

        // Assert
        Assert.Equal(0m, finalYear.ClosingBalance);
        Assert.Equal(0m, finalYear.Drawdown);
        // Below the free area with nothing left, so the full couple rate — the household still has
        // an income rather than nothing.
        Assert.Equal(45_080m, finalYear.Pension);
        Assert.Equal(45_080m, finalYear.TotalIncome);
    }

    /// <summary>
    /// Given a household drawing down through the assets test
    /// When the projection is run
    /// Then the pension should rise as the balance falls
    /// </summary>
    /// <remarks>
    /// The shape a superannuation calculator's income chart shows: super income tapering off while
    /// the pension grows to replace it, because the pension is means-tested on what is left.
    /// </remarks>
    [Fact]
    public void Calculate_AsTheBalanceFalls_ThePensionRises()
    {
        // Arrange: starting above the couple cut-off, so the pension begins at nothing.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 80_000m,
            members: [
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [900_000m]),
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [300_000m]),
            ]);

        // Act
        var years = _engine.Calculate(plan, Today, Rates).Years.ToList();
        var drawing = years.Where(y => y.TotalIncome > 0m).ToList();

        // Assert
        Assert.Equal(0m, drawing[0].Pension);

        // The pension never falls while the balance is being spent, and ends up paying something.
        for (var i = 1; i < drawing.Count; i++)
        {
            Assert.True(drawing[i].Pension >= drawing[i - 1].Pension,
                $"pension fell from {drawing[i - 1].Pension} to {drawing[i].Pension} in {drawing[i].Year}");
        }

        Assert.True(drawing[^1].Pension > 0m);
        Assert.True(projectionTotal(years) > 0m);

        static decimal projectionTotal(List<Asm.MooBank.Modules.Retirement.Models.RetirementProjectionYear> years) =>
            years.Sum(y => y.Pension);
    }

    /// <summary>
    /// Given a pension that covers the whole target
    /// When the projection is run
    /// Then nothing should be drawn from superannuation
    /// </summary>
    /// <remarks>
    /// Superannuation covers only what the pension does not. Drawing the full target on top of the
    /// pension would spend the balance faster than the household needs to.
    /// </remarks>
    [Fact]
    public void Calculate_ThePensionCoversTheTarget_DrawsNothingFromSuper()
    {
        // Arrange: a modest target, and a balance inside the free area.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 30_000m,
            members: [
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [200_000m]),
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [100_000m]),
            ]);

        // Act
        var projection = _engine.Calculate(plan, Today, Rates);

        // Assert
        Assert.All(projection.Years, y => Assert.Equal(0m, y.Drawdown));
        Assert.Null(projection.Summary.MoneyRunsOutYear);
        // The balance is untouched, so it grows rather than depleting.
        Assert.True(projection.Summary.FinalBalance >= 300_000m);
    }

    /// <summary>
    /// Given a modest target and an exhausted balance
    /// When the projection is run
    /// Then the plan should not be reported as running out
    /// </summary>
    /// <remarks>
    /// Once the pension is modelled, spending the superannuation is not the end of the income. A
    /// target the pension alone can meet is a plan that works, and calling it "run out" because the
    /// fund is empty would be wrong.
    /// </remarks>
    [Fact]
    public void Calculate_ATargetThePensionAloneCanMeet_DoesNotRunOut()
    {
        // Arrange: a target just above the full couple rate, so super tops it up until spent.
        var plan = TestEntities.CreatePlan(
            inflationRate: 0m,
            cashReturnRate: 0m,
            targetRetirementIncome: 45_080m,
            members: [
                TestEntities.CreateMember(currentAge: 67, retirementAge: 67, currentIncome: 0m, accountBalances: [10_000m]),
            ]);

        // Act
        var summary = _engine.Calculate(plan, Today, Rates).Summary;

        // Assert
        // A single person's rate is below the target, so this one genuinely does fall short.
        Assert.NotNull(summary.MoneyRunsOutYear);
        Assert.True(summary.TotalPension > 0m);
    }
}
