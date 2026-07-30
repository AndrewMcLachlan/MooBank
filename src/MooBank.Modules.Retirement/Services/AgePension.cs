namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// The Age Pension rates a projection runs under.
/// </summary>
/// <param name="EligibilityAge">The age at which a person becomes eligible.</param>
/// <param name="MaxAnnualSingle">The most a single person can receive in a year.</param>
/// <param name="MaxAnnualCouple">The most a couple can receive in a year between them.</param>
/// <param name="AssetsFreeAreaSingle">Assets a single homeowner may hold before the pension reduces.</param>
/// <param name="AssetsFreeAreaCouple">Assets a homeowner couple may hold before the pension reduces.</param>
/// <param name="AssetsTaperRate">How much of each dollar above the free area is taken off each year.</param>
public readonly record struct AgePensionRates(
    int EligibilityAge,
    decimal MaxAnnualSingle,
    decimal MaxAnnualCouple,
    decimal AssetsFreeAreaSingle,
    decimal AssetsFreeAreaCouple,
    decimal AssetsTaperRate)
{
    /// <summary>
    /// No pension at all: used when no rates have been recorded, so a projection runs on
    /// superannuation alone rather than failing.
    /// </summary>
    public static AgePensionRates None => new(Int32.MaxValue, 0m, 0m, 0m, 0m, 0m);
}

/// <summary>
/// Works out what a household is entitled to from the Age Pension in a given year.
/// </summary>
/// <remarks>
/// <para>
/// Only the assets test is modelled, and this is a deliberate simplification worth being explicit
/// about. The real pension applies an assets test and an income test and pays whichever gives less.
/// For a retiree whose assets are mostly superannuation the assets test is the binding one by a wide
/// margin — at a million-dollar couple balance it gives a few thousand a year where the income test,
/// on deemed earnings, gives close to forty. The income test only binds with substantial income from
/// outside super, which a superannuation projection does not model anyway. Adding deeming would about
/// double the settings to be maintained and change almost no answer.
/// </para>
/// <para>
/// What that costs in accuracy: the pension here is the assets-test figure, so a household with
/// large non-super income would see more pension than they would actually get. Assets counted are
/// the superannuation balances only — the family home is genuinely exempt, but cars, contents and
/// savings outside super are not, and leaving them out understates assets and so overstates the
/// pension.
/// </para>
/// <para>
/// A projection is therefore indicative arithmetic on stated assumptions, not a prediction, and
/// certainly not advice.
/// </para>
/// </remarks>
internal static class AgePension
{
    /// <summary>
    /// The household's entitlement for a year.
    /// </summary>
    /// <param name="rates">The rates in force, already indexed to the year being projected.</param>
    /// <param name="ages">Each member's age in that year.</param>
    /// <param name="assessableAssets">The household's total assessable assets.</param>
    /// <remarks>
    /// The rate is per eligible person, so a couple where only one has reached pension age receives
    /// half the couple rate — while the means test still counts everything they hold between them,
    /// which is how the real test works.
    /// </remarks>
    public static decimal ForYear(AgePensionRates rates, IReadOnlyCollection<int> ages, decimal assessableAssets)
    {
        var eligible = ages.Count(age => age >= rates.EligibilityAge);

        if (eligible == 0) return 0m;

        // A household of two is treated as a couple, of one as single. The plan models a household,
        // so its membership is what decides this rather than a separate setting.
        var isCouple = ages.Count > 1;

        var perPerson = isCouple ? rates.MaxAnnualCouple / 2m : rates.MaxAnnualSingle;
        var maximum = perPerson * eligible;

        var freeArea = isCouple ? rates.AssetsFreeAreaCouple : rates.AssetsFreeAreaSingle;
        var excess = Math.Max(0m, assessableAssets - freeArea);
        var reduction = excess * rates.AssetsTaperRate;

        return Math.Max(0m, maximum - reduction);
    }

    /// <summary>
    /// The same rates expressed in the money of a later year, so they keep pace with the nominal
    /// figures a projection works in.
    /// </summary>
    /// <remarks>
    /// The real rates and thresholds are indexed to inflation, so holding them fixed in nominal terms
    /// would shrink the pension away to nothing over a thirty-year projection — and make a plan look
    /// far worse than it is. The eligibility age is not indexed; it is set by legislation.
    /// </remarks>
    public static AgePensionRates Indexed(AgePensionRates rates, decimal indexation) =>
        rates with
        {
            MaxAnnualSingle = rates.MaxAnnualSingle * indexation,
            MaxAnnualCouple = rates.MaxAnnualCouple * indexation,
            AssetsFreeAreaSingle = rates.AssetsFreeAreaSingle * indexation,
            AssetsFreeAreaCouple = rates.AssetsFreeAreaCouple * indexation,
        };
}
