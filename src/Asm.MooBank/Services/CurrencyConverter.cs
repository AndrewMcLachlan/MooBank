using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace Asm.MooBank.Services;

public interface ICurrencyConverter
{
    decimal? Convert(decimal amount, string from);
}

public class CurrencyConverter(IReferenceDataRepository referenceDataRepository, User user, HybridCache cache) : ICurrencyConverter
{
    private readonly static HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(12),
    };

    public decimal? Convert(decimal amount, string from)
    {
        var to = user.Currency;

        var rate = GetExchangeRate(from, to).Result;

        if (rate == null) return null;

        return amount * rate.Value;
    }

    private async Task<decimal?> GetExchangeRate(string from, string to)
    {
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase)) return 1;

        var rates = await cache.GetOrCreateAsync(
            CacheKeys.ReferenceData.ExchangeRates, 
            async ct => await referenceDataRepository.GetExchangeRates(ct),
            CacheOptions,
            [CacheKeys.ReferenceData.CacheTag],
            CancellationToken.None);

        var rate = rates?.Where(er => er.From == from && er.To == to).SingleOrDefault();

        if (rate != null) return rate.Rate;

        rate = rates?.Where(er => er.From == to && er.To == from).SingleOrDefault();

        if (rate != null) return rate.ReverseRate;

        return null;
    }
}
