using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

/// <summary>
/// The Age Pension rates and thresholds in force from a given date.
/// </summary>
/// <remarks>
/// Reference data rather than a plan setting: the figures are national, so they apply equally to
/// every plan. They are held as a dated series because Services Australia reindexes them twice a
/// year, in March and September, and a projection run today should use the ones in force today.
///
/// There is no official feed to read them from — they are published on the Services Australia site
/// and in PDFs — so they are entered by hand and must be checked against the current published
/// rates.
///
/// The thresholds recorded here are the homeowner ones. Non-homeowners have considerably higher
/// asset free areas, which is not modelled; a household that does not own its home should raise the
/// free areas to their own figures.
/// </remarks>
[PrimaryKey(nameof(Id))]
public class PensionRate([DisallowNull] int id) : KeyedEntity<int>(id)
{
    public PensionRate() : this(default) { }

    /// <summary>
    /// The date these rates took effect. The projection uses the most recent set on or before today.
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// The age at which a person becomes eligible, currently 67.
    /// </summary>
    public int EligibilityAge { get; set; }

    /// <summary>
    /// The most a single person can receive in a year, including supplements.
    /// </summary>
    [Precision(18, 2)]
    public decimal MaxAnnualSingle { get; set; }

    /// <summary>
    /// The most a couple can receive in a year between them, including supplements.
    /// </summary>
    [Precision(18, 2)]
    public decimal MaxAnnualCouple { get; set; }

    /// <summary>
    /// Assets a single homeowner may hold before the pension starts reducing.
    /// </summary>
    [Precision(18, 2)]
    public decimal AssetsFreeAreaSingle { get; set; }

    /// <summary>
    /// Assets a homeowner couple may hold between them before the pension starts reducing.
    /// </summary>
    [Precision(18, 2)]
    public decimal AssetsFreeAreaCouple { get; set; }

    /// <summary>
    /// How much of each dollar above the free area is taken off the pension each year.
    /// </summary>
    /// <remarks>
    /// Published as $3 a fortnight for every $1,000 over, which is $78 a year per $1,000 — a rate of
    /// 0.078 on the excess.
    /// </remarks>
    [Precision(6, 4)]
    public decimal AssetsTaperRate { get; set; }
}
