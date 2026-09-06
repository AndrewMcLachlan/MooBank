using Asm.MooBank.Domain.Entities.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// The return a projection assumes for each investment strategy.
/// </summary>
/// <param name="rates">The rate recorded against each named strategy.</param>
/// <remarks>
/// A named strategy's rate is reference data, so correcting it moves every plan that uses it at
/// once. A custom rate belongs to the member who chose it and travels with them instead.
/// </remarks>
public sealed class GrowthStrategyRates(IReadOnlyDictionary<GrowthStrategy, decimal> rates)
{
    /// <summary>
    /// Nothing recorded at all, so every strategy earns nought.
    /// </summary>
    /// <remarks>
    /// A projection that returns nothing is visibly wrong, where one quietly falling back to a rate
    /// nobody chose would not be.
    /// </remarks>
    public static GrowthStrategyRates None { get; } = new(new Dictionary<GrowthStrategy, decimal>());

    /// <summary>
    /// The nominal return a member earns on the strategy they are on.
    /// </summary>
    /// <param name="strategy">The strategy chosen.</param>
    /// <param name="customRate">The member's own rate, read only when the strategy is Custom.</param>
    public decimal For(GrowthStrategy strategy, decimal? customRate) =>
        strategy == GrowthStrategy.Custom ? customRate ?? 0m : rates.GetValueOrDefault(strategy);

    /// <summary>
    /// The return on the part of a balance held in the cash bucket.
    /// </summary>
    public decimal Cash => rates.GetValueOrDefault(GrowthStrategy.Cash);
}

/// <summary>
/// Reads the return assumptions a projection runs under.
/// </summary>
public interface IGrowthStrategyRateReader
{
    Task<GrowthStrategyRates> Current(CancellationToken cancellationToken = default);
}

internal class GrowthStrategyRateReader(IQueryable<GrowthStrategyRate> rates) : IGrowthStrategyRateReader
{
    public async Task<GrowthStrategyRates> Current(CancellationToken cancellationToken = default) =>
        new(await rates
            .Where(r => r.Rate != null)
            .ToDictionaryAsync(r => r.Strategy, r => r.Rate!.Value, cancellationToken));
}
