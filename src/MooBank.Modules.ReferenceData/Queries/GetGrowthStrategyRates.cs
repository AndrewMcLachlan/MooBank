using System.ComponentModel;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Modules.ReferenceData.Models;

namespace Asm.MooBank.Modules.ReferenceData.Queries;

[DisplayName("GetGrowthStrategyRates")]
public record GetGrowthStrategyRates : IQuery<IEnumerable<GrowthStrategyRates>>;

internal class GetGrowthStrategyRatesHandler(IQueryable<GrowthStrategyRate> rates) : IQueryHandler<GetGrowthStrategyRates, IEnumerable<GrowthStrategyRates>>
{
    /// <remarks>
    /// In rate order, so the list reads as the risk ladder it is. Custom has no rate and sits first,
    /// which is also where it belongs: it is the option that opts out of the ladder.
    /// </remarks>
    public async ValueTask<IEnumerable<GrowthStrategyRates>> Handle(GetGrowthStrategyRates query, CancellationToken cancellationToken) =>
        await rates
            .OrderBy(r => r.Rate ?? -1m)
            .Select(r => new GrowthStrategyRates
            {
                Strategy = r.Strategy,
                Description = r.Description,
                Rate = r.Rate,
            })
            .ToListAsync(cancellationToken);
}
