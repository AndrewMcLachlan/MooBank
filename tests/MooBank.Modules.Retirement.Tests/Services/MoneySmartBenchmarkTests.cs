using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Asm.MooBank.Modules.Retirement.Tests.Support;

namespace Asm.MooBank.Modules.Retirement.Tests.Services;

/// <summary>
/// MooBank's projection against ASIC's MoneySmart superannuation calculator, on the same inputs.
/// </summary>
/// <remarks>
/// <para>
/// A projection is only as trustworthy as something independent agrees with, and MoneySmart is the
/// obvious comparator: it is the calculator most Australians will have checked their own numbers
/// against. The figures below are from a real MoneySmart run, taken from its response rather than
/// read off the page, so they can be pinned exactly.
/// </para>
/// <para>
/// The two engines do not agree to the cent and are not meant to. What this guards is the size of
/// the disagreement: each remaining difference is understood and listed, and if a change to the
/// engine widens the gap beyond them, that is a regression worth stopping.
/// </para>
/// <para>
/// Known and deliberate differences, in the order they matter:
/// <list type="bullet">
/// <item>MoneySmart retires at the end of the financial year in which the age is reached, giving it
/// most of an extra year of contributions and compounding.</item>
/// <item>MooBank leaves the first projected year unindexed — the salary entered is what is earned
/// that year — while MoneySmart applies a rise before it.</item>
/// <item>MooBank holds a cash bucket in the years before retirement, which MoneySmart does not
/// model at all.</item>
/// </list>
/// </para>
/// </remarks>
[Trait("Category", "Unit")]
public class MoneySmartBenchmarkTests
{
    /// <summary>The day the MoneySmart run was made, so both engines project the same span.</summary>
    private static readonly DateOnly Today = new(2026, 9, 6);

    /// <summary>What MoneySmart reported for the balance at retirement, in today's dollars.</summary>
    private const decimal MoneySmartBalanceAtRetirement = 1_702_500m;

    /// <summary>
    /// The band the known differences put the two engines in: currently 5.4% apart, with the
    /// retirement boundary worth around 4.7% of it and the unindexed first year most of the rest.
    /// </summary>
    /// <remarks>
    /// A band rather than a ceiling, because closing the gap unexpectedly is as much a signal as
    /// widening it — it would mean one of the deliberate differences had been removed without the
    /// list above being updated. Move either bound only alongside an entry saying why.
    /// </remarks>
    private const decimal SmallestExpectedDifference = 0.04m;

    private const decimal LargestExpectedDifference = 0.07m;

    private readonly RetirementProjectionEngine _engine = new();

    /// <summary>
    /// The MoneySmart scenario, as its own request payload describes it: 2.5% inflation with a
    /// further 1.2% for living standards, 6.8% accumulating and 6.1% once retired, a 12% employer
    /// contribution and 2.06% of salary sacrificed.
    /// </summary>
    private static Asm.MooBank.Domain.Entities.Retirement.RetirementPlan Plan() =>
        TestEntities.CreatePlan(
            inflationRate: 0.037m,
            superGuaranteeRate: 0.12m,
            contributionsTaxRate: 0.15m,
            lifeExpectancy: 92,
            targetRetirementIncome: 184_300m,
            cashBucketYears: 2,
            members: [
                TestEntities.CreateMember(
                    name: "Andy", currentAge: 47, retirementAge: 67,
                    currentIncome: 231_000m, salarySacrifice: 4_780m,
                    growthStrategy: GrowthStrategy.Custom, customReturnRate: 0.068m,
                    retirementGrowthStrategy: GrowthStrategy.Custom, retirementCustomReturnRate: 0.061m,
                    annualFees: 402m, insurancePremium: 456m,
                    accountBalances: [511_538.57m]),
            ]);

    /// <summary>
    /// Given the MoneySmart scenario
    /// When MooBank projects it
    /// Then the balance at retirement should differ by only what the known differences explain
    /// </summary>
    [Fact]
    public void Calculate_TheMoneySmartScenario_DiffersOnlyByWhatIsExplained()
    {
        // Act
        var summary = _engine
            .Calculate(Plan(), Today, AgePensionRates.None, TestEntities.StrategyRates(Plan(), 0.037m), TestEntities.MinimumDrawdown())
            .Summary;

        // Assert
        var ours = summary.BalanceAtRetirementInTodaysDollars;
        var difference = Math.Abs(ours - MoneySmartBalanceAtRetirement) / MoneySmartBalanceAtRetirement;

        Assert.True(difference >= SmallestExpectedDifference && difference <= LargestExpectedDifference,
            $"MooBank projected {ours:N0} against MoneySmart's {MoneySmartBalanceAtRetirement:N0}, a difference of " +
            $"{difference:P1}, outside the {SmallestExpectedDifference:P0}-{LargestExpectedDifference:P0} the known " +
            "differences account for. Either something has changed the projection, or one of those differences has gone.");
    }

    /// <summary>
    /// Given the same scenario
    /// When MooBank projects it
    /// Then it should read below MoneySmart, not above
    /// </summary>
    /// <remarks>
    /// Every known difference runs one way: MoneySmart gets a longer accumulation and an earlier
    /// pay rise, and pays no cash-bucket cost. MooBank coming out ahead would mean one of them has
    /// been removed, or something new is inflating the projection.
    /// </remarks>
    [Fact]
    public void Calculate_TheMoneySmartScenario_ReadsBelowIt()
    {
        var summary = _engine
            .Calculate(Plan(), Today, AgePensionRates.None, TestEntities.StrategyRates(Plan(), 0.037m), TestEntities.MinimumDrawdown())
            .Summary;

        Assert.True(summary.BalanceAtRetirementInTodaysDollars < MoneySmartBalanceAtRetirement,
            $"MooBank projected {summary.BalanceAtRetirementInTodaysDollars:N0}, above MoneySmart's {MoneySmartBalanceAtRetirement:N0}.");
    }
}
