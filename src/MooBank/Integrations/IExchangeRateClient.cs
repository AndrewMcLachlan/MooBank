namespace Asm.MooBank.ExchangeRateApi;

public interface IExchangeRateClient
{
    Task<IDictionary<string, decimal>> GetExchangeRates(string from, CancellationToken cancellationToken = default);
}
