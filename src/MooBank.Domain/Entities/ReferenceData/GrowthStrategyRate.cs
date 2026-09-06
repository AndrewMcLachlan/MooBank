using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

/// <summary>
/// The assumed long-run nominal return for one investment strategy.
/// </summary>
/// <remarks>
/// Reference data rather than a plan setting: a return assumption describes a market, not a
/// household, so one set applies to every plan and a correction reaches them all at once. Nothing is
/// stored against a projection — each is recalculated from whatever these say when it runs — so
/// unlike <see cref="PensionRate"/> there is no dated series to choose from.
///
/// The seeded figures are ASIC's, published net of investment fees and of tax on fund earnings.
/// Administration fees and insurance are charged separately, against the member.
///
/// <see cref="GrowthStrategy.Custom"/> has no rate here: it means whatever figure a member chose,
/// which is held on the member.
/// </remarks>
[PrimaryKey(nameof(Strategy))]
public class GrowthStrategyRate
{
    [Column("Id")]
    public GrowthStrategy Strategy { get; set; }

    [MaxLength(50)]
    public required string Description { get; set; }

    [Precision(6, 4)]
    public decimal? Rate { get; set; }
}
