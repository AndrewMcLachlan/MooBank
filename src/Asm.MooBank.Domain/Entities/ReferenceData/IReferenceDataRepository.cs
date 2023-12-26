namespace Asm.MooBank.Domain.Entities.ReferenceData;

public interface IReferenceDataRepository
{
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(string internationalSymbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockPriceHistory>> GetStockPrices(string symbol, string exchange, CancellationToken cancellationToken = default);

    StockPriceHistory AddStockPrice(StockPriceHistory stockPrice);
}
