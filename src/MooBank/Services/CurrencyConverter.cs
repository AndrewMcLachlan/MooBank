using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Services;

public interface ICurrencyConverter
{
    Task<decimal?> Convert(decimal amount, string from, CancellationToken cancellationToken = default);
}

public class CurrencyConverter(IReferenceDataRepository referenceDataRepository, User user, HybridCache cache) : ICurrencyConverter
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(12),
    };

    public async Task<decimal?> Convert(decimal amount, string from, CancellationToken cancellationToken = default)
    {
        var to = user.Currency;

        var rate = await GetExchangeRate(from, to, cancellationToken);

        if (rate == null) return null;

        return amount * rate.Value;
    }

    private async Task<decimal?> GetExchangeRate(string from, string to, CancellationToken cancellationToken)
    {
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase)) return 1;

        var rates = await cache.GetOrCreateAsync(
            CacheKeys.ReferenceData.ExchangeRates,
            async ct => await referenceDataRepository.GetExchangeRates(ct),
            CacheOptions,
            [CacheKeys.ReferenceData.CacheTag],
            cancellationToken);

        var rate = rates?.Where(er => er.From == from && er.To == to).SingleOrDefault();

        if (rate != null) return rate.Rate;

        rate = rates?.Where(er => er.From == to && er.To == from).SingleOrDefault();

        if (rate != null) return rate.ReverseRate;

        return null;
    }
}
