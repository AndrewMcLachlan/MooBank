using System.ComponentModel;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Modules.ReferenceData.Models;

namespace Asm.MooBank.Modules.ReferenceData.Queries;

[DisplayName("GetPensionRates")]
public record GetPensionRates : IQuery<IEnumerable<PensionRates>>;

internal class GetPensionRatesHandler(IQueryable<PensionRate> rates) : IQueryHandler<GetPensionRates, IEnumerable<PensionRates>>
{
    /// <remarks>
    /// Newest first, since that is the set in force and the one a reader wants; the older rows are
    /// history.
    /// </remarks>
    public async ValueTask<IEnumerable<PensionRates>> Handle(GetPensionRates query, CancellationToken cancellationToken) =>
        await rates
            .OrderByDescending(r => r.EffectiveFrom)
            .Select(r => new PensionRates
            {
                Id = r.Id,
                EffectiveFrom = r.EffectiveFrom,
                EligibilityAge = r.EligibilityAge,
                MaxAnnualSingle = r.MaxAnnualSingle,
                MaxAnnualCouple = r.MaxAnnualCouple,
                AssetsFreeAreaSingle = r.AssetsFreeAreaSingle,
                AssetsFreeAreaCouple = r.AssetsFreeAreaCouple,
                AssetsTaperRate = r.AssetsTaperRate,
            })
            .ToListAsync(cancellationToken);
}
