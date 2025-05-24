using System.Xml.Linq;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Abs;

public interface IAbsClient
{
    Task<IEnumerable<CpiChange>> GetCpiChanges(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default);
}

internal class AbsClient(IHttpClientFactory httpClientFactory, ILogger<AbsClient> logger) : IAbsClient
{
    public async Task<IEnumerable<CpiChange>> GetCpiChanges(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default)
    {
        XDocument? data = await GetData(["ABS", "CPI"], ["2", "10001", "10", "1", "Q"], startDate, endDate, cancellationToken);

        if (data == null)
        {
            logger.LogWarning("No data returned from ABS API");
            return [];
        }

        return ParseCpiChanges(data);
    }

    private async Task<XDocument?> GetData(string[] source, string[] dimensions, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken = default)
    {

        try
        {
            var httpClient = httpClientFactory.CreateClient("abs");

            string url = $"{String.Join(",", source)}/{String.Join('.', dimensions)}";

            string startDateQuery = startDate.HasValue ? $"startPeriod={startDate.Value:yyyy-MM-dd}" : String.Empty;
            string endDateQuery = endDate.HasValue ? $"endPeriod={endDate.Value:yyyy-MM-dd}" : String.Empty;


            string query = $"?{startDateQuery}&{endDateQuery}".TrimEnd('&','?');


            var response = await httpClient.GetAsync(url + query, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("No data returned from ABS API");
                return null;
            }
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return XDocument.Parse(content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting data from ABS API");
            return null;
        }
    }

    private IEnumerable<CpiChange> ParseCpiChanges(XDocument data)
    {
        var ns = XNamespace.Get("http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
        var obs = data.Descendants(ns + "Obs");

        foreach (var o in obs)
        {
            if (!Quarter.TryParse(o.Element(ns + "ObsDimension")?.Attribute("value")?.Value, out Quarter quarter) ||
                !Decimal.TryParse(o.Element(ns + "ObsValue")?.Attribute("value")?.Value, out decimal changePercent))
            {
                logger.LogWarning("Invalid data format in ABS API response");
                continue;
            }

            yield return new CpiChange
            {
                Quarter = quarter,
                ChangePercent = changePercent
            };
        }
    }
}
