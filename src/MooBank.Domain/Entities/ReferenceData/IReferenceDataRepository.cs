using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.ReferenceData;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(DateOnly date, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<ExchangeRate>> GetExchangeRates(CancellationToken cancellationToken = default);

    Task<IEnumerable<CpiChange>> GetCpiChanges(CancellationToken cancellationToken = default);

    Task<StockPriceHistory> AddStockPrice(StockPriceHistory stockPrice, CancellationToken cancellationToken = default);

    ExchangeRate AddExchangeRate(ExchangeRate exchangeRate);

    CpiChange AddCpiChange(CpiChange cpiChange);
}
