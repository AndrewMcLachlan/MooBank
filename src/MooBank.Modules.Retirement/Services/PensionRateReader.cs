using Asm.MooBank.Domain.Entities.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// Reads the Age Pension rates in force for a projection.
/// </summary>
public interface IPensionRateReader
{
    Task<AgePensionRates> Current(DateOnly asAt, CancellationToken cancellationToken = default);
}

internal class PensionRateReader(IQueryable<PensionRate> rates) : IPensionRateReader
{
    /// <summary>
    /// The most recent set of rates that had taken effect by the given date.
    /// </summary>
    /// <remarks>
    /// Dated rather than latest-wins, so entering next March's rates ahead of time does not change
    /// today's projections. If nothing has been recorded the projection runs on superannuation alone
    /// rather than failing — a plan is still useful without the pension, and a missing row is a
    /// settings gap rather than an error.
    /// </remarks>
    public async Task<AgePensionRates> Current(DateOnly asAt, CancellationToken cancellationToken = default)
    {
        var current = await rates
            .Where(r => r.EffectiveFrom <= asAt)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        return current is null
            ? AgePensionRates.None
            : new AgePensionRates(
                current.EligibilityAge,
                current.MaxAnnualSingle,
                current.MaxAnnualCouple,
                current.AssetsFreeAreaSingle,
                current.AssetsFreeAreaCouple,
                current.AssetsTaperRate);
    }
}
