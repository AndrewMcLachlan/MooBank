using System.ComponentModel;

namespace Asm.MooBank.Modules.ReferenceData.Models;

/// <summary>
/// The Age Pension rates and thresholds in force from a date.
/// </summary>
/// <remarks>
/// National figures, so they are reference data rather than a setting on any one plan. Services
/// Australia reindexes them each March and September and publishes no feed to read them from, so
/// they are entered by hand and need checking against the current published rates.
///
/// The asset free areas are the homeowner ones. Non-homeowners have considerably higher thresholds,
/// which is not modelled separately — such a household should enter their own figures.
/// </remarks>
[DisplayName("PensionRates")]
public record PensionRates
{
    public int Id { get; init; }

    /// <summary>
    /// The date these rates took effect. A projection uses the most recent set on or before the day
    /// it runs, so next March's rates can be entered ahead of time without changing today's answers.
    /// </summary>
    public required DateOnly EffectiveFrom { get; init; }

    /// <summary>The age at which a person becomes eligible.</summary>
    public required int EligibilityAge { get; init; }

    /// <summary>The most a single person can receive in a year, including supplements.</summary>
    public required decimal MaxAnnualSingle { get; init; }

    /// <summary>The most a couple can receive in a year between them, including supplements.</summary>
    public required decimal MaxAnnualCouple { get; init; }

    /// <summary>Assets a single homeowner may hold before the pension starts reducing.</summary>
    public required decimal AssetsFreeAreaSingle { get; init; }

    /// <summary>Assets a homeowner couple may hold between them before the pension starts reducing.</summary>
    public required decimal AssetsFreeAreaCouple { get; init; }

    /// <summary>
    /// How much of each dollar above the free area is taken off the pension each year. Published as
    /// $3 a fortnight per $1,000, which is $78 a year per $1,000 — a rate of 0.078.
    /// </summary>
    public required decimal AssetsTaperRate { get; init; }
}
