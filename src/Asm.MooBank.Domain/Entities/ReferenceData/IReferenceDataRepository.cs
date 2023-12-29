namespace Asm.MooBank.Domain.Entities.ReferenceData;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default);

    StockPriceHistory AddStockPrice(StockPriceHistory stockPrice);

    Task<ExchangeRate?> GetExchangeRate(string from, string to, CancellationToken cancellationToken = default);

    ExchangeRate AddExchangeRate(ExchangeRate exchangeRate);

    Task<IEnumerable<ExchangeRate>> GetExchangeRates(CancellationToken cancellationToken = default);
}
