using System.Net.Http.Json;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Eodhd;

public interface IStockPriceClient
{
    Task<decimal?> GetPriceAsync(StockSymbol symbol);
}

public class StockPriceClient(IHttpClientFactory httpClientFactory, IOptions<EodhdConfig> config, ILogger<StockPriceClient> logger) : IStockPriceClient
{
    private const string UrlFormat = "https://eodhd.com/api/eod/{0}?api_token={1}&from={2}&to={3}&fmt=json";

    public Task<decimal?> GetPriceAsync(StockSymbol symbol) => GetPriceAsync(symbol.ToString());

    public async Task<decimal?> GetPriceAsync(string symbol)
    {
        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        string url = String.Format(UrlFormat, symbol, config.Value.ApiKey, yesterday, yesterday);

        try
        {
            var httpClient = httpClientFactory.CreateClient("eodhd");

            IEnumerable<StockPrice>? prices = await httpClient.GetFromJsonAsync<IEnumerable<StockPrice>>(url);

            if (prices == null || !prices.Any())
            {
                logger.LogWarning("No prices returned");
                return null;
            }

            return prices.OrderByDescending(p => p.Date).Select(p => p.AdjustedClose).First();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting stock prices");
            return null;
        }
    }
}
