using Asm.Domain;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Eodhd;
using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;
public interface IStockPriceService
{
    Task Update();
}

public class StockPriceService(IUnitOfWork unitOfWork, IStockHoldingRepository stockHoldingRepository, IReferenceDataRepository referenceDataRepository, IStockPriceClient stockPriceClient, ILogger<StockPriceService> logger) : IStockPriceService
{
    public async Task Update()
    {
        logger.LogInformation("Updating stock prices...");

        var stocks = await stockHoldingRepository.Get();

        var symbols = stocks.Select(s => s.Symbol).Distinct();

        Dictionary<string, decimal> prices = [];

        foreach (var symbol in symbols)
        {
            var price = await stockPriceClient.GetPriceAsync(symbol);
            if (price == null) continue;

            prices.Add(symbol, price.Value);
        }

        foreach (var stock in stocks)
        {
            if (!prices.ContainsKey(stock.Symbol))
            {
                logger.LogWarning("Unable to set the prices for {symbol}", stock.Symbol);
                continue;
            }

            stock.CurrentPrice = prices[stock.Symbol];
            stock.LastUpdated = DateTimeOffset.UtcNow;
        }

        var existingPrices = await referenceDataRepository.GetStockPrices(DateOnlyExtensions.Today().AddDays(-1));

        foreach (var price in prices.Where(p => !existingPrices.Select(e => e.Symbol).Contains(p.Key)))
        {
            logger.LogInformation("Setting {symbol} to {price}", price.Key, price.Value);
            referenceDataRepository.AddStockPrice(new StockPriceHistory()
            {
                Symbol = price.Key,
                Date = DateOnlyExtensions.Today().AddDays(-1),
                Price = price.Value,
            });
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Completed stock price update");
    }
}
