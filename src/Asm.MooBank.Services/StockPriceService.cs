using Asm.Domain;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Eodhd;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;
public interface IStockPriceService
{
    Task Update();
}

public class StockPriceService(IUnitOfWork unitOfWork, IStockHoldingRepository stockHoldingRepository, IStockPriceClient stockPriceClient, ILogger<StockPriceService> logger) : IStockPriceService
{
    public async Task Update()
    {
        logger.LogInformation("Updating stock prices...");

        var stocks = await stockHoldingRepository.Get();

        var symbols = stocks.Select(s => s.InternationalSymbol).Distinct();

        Dictionary<string, decimal> prices = [];

        foreach (var symbol in symbols)
        {
            var price = await stockPriceClient.GetPriceAsync(symbol);
            if (price == null) continue;

            prices.Add(symbol, price.Value);
        }

        foreach (var stock in stocks)
        {
            if (!prices.ContainsKey(stock.InternationalSymbol))
            {
                logger.LogWarning("Unable to set the prices for {symbol}", stock.InternationalSymbol);
                continue;
            }

            stock.CurrentPrice = prices[stock.InternationalSymbol];
            logger.LogInformation("Setting {symbol} to {price}", stock.InternationalSymbol, stock.CurrentPrice);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Completed stock price update");
    }
}
