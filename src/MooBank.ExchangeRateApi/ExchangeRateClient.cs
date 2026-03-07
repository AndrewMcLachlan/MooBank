using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.ExchangeRateApi;

public interface IExchangeRateClient
{
    Task<decimal?> GetExchangeRate(string from, string to, CancellationToken cancellationToken = default);

    Task<IDictionary<string, decimal>> GetExchangeRates(string from, CancellationToken cancellationToken = default);
}

internal class ExchangeRateClient(IHttpClientFactory httpClientFactory, IOptions<ExchangeRateApiConfig> config, ILogger<ExchangeRateClient> logger) : IExchangeRateClient
{

    public async Task<IDictionary<string, decimal>> GetExchangeRates(string from, CancellationToken cancellationToken = default)
    {
        string url = from;

        try
        {
            var httpClient = httpClientFactory.CreateClient("ExchangeRateApi");

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.Value.ApiKey);

            ExchangeRateApiResponse? rates = await httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(url, cancellationToken);

            return rates?.ConversionRates ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rates");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<decimal?> GetExchangeRate(string from, string to, CancellationToken cancellationToken = default)
    {
        string url = from;

        try
        {
            var httpClient = httpClientFactory.CreateClient("ExchangeRateApi");

            ExchangeRateApiResponse? rates = await httpClient.GetFromJsonAsync<ExchangeRateApiResponse>(url, cancellationToken);

            decimal? rate = rates?.ConversionRates.Where(r => r.Key.Equals(to, StringComparison.OrdinalIgnoreCase)).Select(r => r.Value).SingleOrDefault();

            if (rate == null)
            {
                logger.LogWarning("Rate not returned");
            }

            return rate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rates");
            return null;
        }
    }
}
