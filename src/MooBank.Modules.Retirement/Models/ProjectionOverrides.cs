using System.ComponentModel;

namespace Asm.MooBank.Modules.Retirement.Models;

/// <summary>
/// Values to run a projection under instead of the ones saved on the plan.
/// </summary>
/// <remarks>
/// This is what the tweak sliders send. Nothing here is persisted: the projection is recalculated
/// under the supplied values and the plan is left untouched, so an override lasts only as long as
/// the caller keeps sending it. Saving the plan is a separate, deliberate act.
///
/// Every value is optional and falls back to the plan.
/// </remarks>
[DisplayName("RetirementProjectionOverrides")]
public sealed record ProjectionOverrides
{
    public decimal? ExpectedReturnRate { get; init; }

    public decimal? InflationRate { get; init; }

    public decimal? SuperGuaranteeRate { get; init; }

    public decimal? ContributionsTaxRate { get; init; }

    public int? LifeExpectancy { get; init; }

    public IEnumerable<MemberOverride> Members { get; init; } = [];
}

/// <summary>
/// Values to run one member under instead of the ones saved against them.
/// </summary>
[DisplayName("RetirementMemberOverride")]
public sealed record MemberOverride
{
    /// <summary>
    /// The member these values apply to. An id that is not on the plan is ignored rather than
    /// treated as an error, so a stale slider cannot fail the whole projection.
    /// </summary>
    public Guid MemberId { get; init; }

    public int? CurrentAge { get; init; }

    public decimal? CurrentIncome { get; init; }

    public decimal? SalarySacrifice { get; init; }

    public int? RetirementAge { get; init; }

    public GrowthStrategy? GrowthStrategy { get; init; }
}
