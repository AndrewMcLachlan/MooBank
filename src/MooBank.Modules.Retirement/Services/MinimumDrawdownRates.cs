using Asm.MooBank.Domain.Entities.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Retirement.Services;

/// <summary>
/// The least an account-based pension must pay out each year, by age.
/// </summary>
/// <param name="bands">Each band's rate, keyed by the age it starts at.</param>
/// <remarks>
/// Legislated rather than assumed, which is what makes it worth modelling: a plan drawing less than
/// the minimum is not a plan anyone is allowed to follow. Above the minimum the household draws
/// what it means to spend; below it, the law draws for them.
/// </remarks>
public sealed class MinimumDrawdownRates(IReadOnlyDictionary<byte, decimal> bands)
{
    private readonly (byte MinAge, decimal Rate)[] _ordered =
        [.. bands.OrderByDescending(b => b.Key).Select(b => (b.Key, b.Value))];

    /// <summary>
    /// No minimum recorded, so nothing is forced out.
    /// </summary>
    /// <remarks>
    /// A projection is still useful without it, and a missing table is a settings gap rather than a
    /// reason to refuse to run — the same treatment the Age Pension rates get.
    /// </remarks>
    public static MinimumDrawdownRates None { get; } = new(new Dictionary<byte, decimal>());

    /// <summary>
    /// The share of the balance someone of this age must draw.
    /// </summary>
    public decimal For(int age)
    {
        foreach (var (minAge, rate) in _ordered)
        {
            if (age >= minAge) return rate;
        }

        return 0m;
    }
}

/// <summary>
/// Reads the minimum drawdown rates a projection runs under.
/// </summary>
public interface IMinimumDrawdownRateReader
{
    Task<MinimumDrawdownRates> Current(CancellationToken cancellationToken = default);
}

internal class MinimumDrawdownRateReader(IQueryable<MinimumDrawdownRate> rates) : IMinimumDrawdownRateReader
{
    public async Task<MinimumDrawdownRates> Current(CancellationToken cancellationToken = default) =>
        new(await rates.ToDictionaryAsync(r => r.MinAge, r => r.Rate, cancellationToken));
}
