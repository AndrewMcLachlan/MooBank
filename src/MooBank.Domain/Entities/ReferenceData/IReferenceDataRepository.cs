
namespace Asm.MooBank.Domain.Entities.ReferenceData;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(DateOnly date, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<ExchangeRate>> GetExchangeRates(CancellationToken cancellationToken = default);

    Task<IEnumerable<CpiChange>> GetCpiChanges(CancellationToken cancellationToken = default);

    /// <summary>
    /// A tracked set of Age Pension rates, for correcting figures already recorded.
    /// </summary>
    Task<PensionRate> GetPensionRate(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// A new, tracked set of Age Pension rates. Its values are set by the caller.
    /// </summary>
    PensionRate AddPensionRate();

    Task<StockPriceHistory> AddStockPrice(StockPriceHistory stockPrice, CancellationToken cancellationToken = default);

    ExchangeRate AddExchangeRate(ExchangeRate exchangeRate);

    CpiChange AddCpiChange(CpiChange cpiChange);
}
