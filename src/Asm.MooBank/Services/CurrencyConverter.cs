using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using LazyCache;

namespace Asm.MooBank.Services;

public interface ICurrencyConverter
{
    decimal? Convert(decimal amount, string from);
}

public class CurrencyConverter(IReferenceDataRepository referenceDataRepository, AccountHolder accountHolder, IAppCache appCache) : ICurrencyConverter
{
    public decimal? Convert(decimal amount, string from)
    {
        var to = accountHolder.Currency;

        var rate = GetExchangeRate(from, to);

        if (rate == null) return null;

        return amount * rate.Value;
    }

    private decimal? GetExchangeRate(string from, string to)
    {
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase)) return 1;

        var rates = appCache.GetOrAddAsync("IReferenceDataRepository-GetExchangeRates", referenceDataRepository.GetExchangeRates, DateTimeOffset.Now.AddHours(12)).Result;

        var rate = rates?.Where(er => er.From == from && er.To == to).SingleOrDefault();

        if (rate != null) return rate.Rate;

        rate = rates?.Where(er => er.From == to && er.To == from).SingleOrDefault();

        if (rate != null) return rate.ReverseRate;

        return null;
    }
}
