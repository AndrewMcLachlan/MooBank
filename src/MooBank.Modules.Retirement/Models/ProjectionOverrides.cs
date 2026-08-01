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

    public decimal? TargetRetirementIncome { get; init; }

    public int? CashBucketYears { get; init; }

    public decimal? CashReturnRate { get; init; }

    public IEnumerable<MemberOverride> Members { get; init; } = [];

    /// <summary>
    /// Members to leave out of this projection entirely, so a household can be seen one person at a
    /// time.
    /// </summary>
    /// <remarks>
    /// A view of the plan, not an edit to it: an excluded member keeps their place on the plan and
    /// comes back the moment the exclusion is dropped. Leaving everyone out is treated the same as
    /// leaving nobody out, since a projection of nobody says nothing.
    ///
    /// Excluding someone changes what the household is, not just what it holds — a couple projected
    /// as one person is assessed at the single Age Pension rate and free area, which is what makes
    /// the answer a genuine "just me" rather than half of a couple's.
    /// </remarks>
    public IEnumerable<Guid> ExcludedMemberIds { get; init; } = [];
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

    public decimal? AnnualFees { get; init; }

    public decimal? InsurancePremium { get; init; }

    public int? RetirementAge { get; init; }

    public GrowthStrategy? GrowthStrategy { get; init; }
}
