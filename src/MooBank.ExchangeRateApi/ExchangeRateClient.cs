using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.ExchangeRateApi;

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
}
