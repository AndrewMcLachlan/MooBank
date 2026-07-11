namespace Asm.MooBank.Eodhd;

public interface IStockPriceClient
{
    Task<decimal?> GetPriceAsync(StockSymbol symbol);
}
