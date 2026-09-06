using System.ComponentModel;

namespace Asm.MooBank.Modules.ReferenceData.Models;

/// <summary>
/// The assumed long-run nominal return for one investment strategy.
/// </summary>
/// <remarks>
/// An assumption about a market rather than a household, so it is reference data: one figure serves
/// every plan and correcting it moves them all together.
///
/// The seeded values are ASIC's, published net of investment fees and of tax on fund earnings —
/// administration fees and insurance are charged separately, against the member. A replacement
/// figure taken from a fund's gross return would double-count both.
///
/// Custom appears here with no rate, because it means whatever figure a member chose.
/// </remarks>
[DisplayName("GrowthStrategyRates")]
public record GrowthStrategyRates
{
    public required GrowthStrategy Strategy { get; init; }

    public required string Description { get; init; }

    /// <summary>
    /// The nominal return, as a rate: 0.064 is 6.4% a year. Absent for Custom.
    /// </summary>
    public decimal? Rate { get; init; }
}
