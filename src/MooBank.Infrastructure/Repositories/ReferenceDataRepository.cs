using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class ReferenceDataRepository(MooBankContext dataContext) : IReferenceDataRepository
{
    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) =>
        await dataContext.ImporterTypes.ToListAsync(cancellationToken);

    public async Task<IEnumerable<StockPriceHistory>> GetStockPrices(DateOnly date, CancellationToken cancellationToken = default) =>
        await dataContext.StockPriceHistory.Where(s => s.Date == date).ToListAsync(cancellationToken);

    public async Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default) =>
       await dataContext.StockPriceHistory.Where(s => s.Symbol == symbol).ToListAsync(cancellationToken);

    public StockPriceHistory AddStockPrice(StockPriceHistory stockPrice)
    {
        // Do not attempt to re-add existing data
        if (dataContext.StockPriceHistory.Any(sp => sp.Symbol == stockPrice.Symbol && sp.Date == stockPrice.Date))
        {
            return stockPrice;
        }

        dataContext.StockPriceHistory.Add(stockPrice);
        return stockPrice;
    }

    public ExchangeRate AddExchangeRate(ExchangeRate exchangeRate)
    {
        dataContext.ExchangeRates.Add(exchangeRate);
        return exchangeRate;
    }

    public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(CancellationToken cancellationToken = default) =>
        await dataContext.ExchangeRates.ToListAsync(cancellationToken);

    public async Task<IEnumerable<CpiChange>> GetCpiChanges(CancellationToken cancellationToken = default) =>
        await dataContext.CpiChanges.ToListAsync(cancellationToken);

    public CpiChange AddCpiChange(CpiChange cpiChange) =>
        dataContext.CpiChanges.Add(cpiChange).Entity;
}
