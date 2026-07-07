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
       await dataContext.StockPriceHistory.Where(s => s.Symbol == symbol.Symbol && s.Exchange == symbol.Exchange).ToListAsync(cancellationToken);

    public async Task<StockPriceHistory> AddStockPrice(StockPriceHistory stockPrice, CancellationToken cancellationToken = default)
    {
        // Do not attempt to re-add existing data
        if (await dataContext.StockPriceHistory.AnyAsync(sp => sp.Symbol == stockPrice.Symbol && sp.Exchange == stockPrice.Exchange && sp.Date == stockPrice.Date, cancellationToken))
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
