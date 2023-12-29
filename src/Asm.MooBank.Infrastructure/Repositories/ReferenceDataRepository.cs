using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class ReferenceDataRepository(MooBankContext dataContext) : IReferenceDataRepository
{
    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) =>
        await dataContext.ImporterTypes.ToListAsync(cancellationToken);

    public async Task<IEnumerable<StockPriceHistory>> GetStockPrices(StockSymbol symbol, CancellationToken cancellationToken = default) =>
       await dataContext.Set<StockPriceHistory>().Where(s => s.Symbol == symbol).ToListAsync(cancellationToken);

    public StockPriceHistory AddStockPrice(StockPriceHistory stockPrice)
    {
        dataContext.Set<StockPriceHistory>().Add(stockPrice);
        return stockPrice;
    }

    public Task<ExchangeRate?> GetExchangeRate(string from, string to, CancellationToken cancellationToken = default) =>
        dataContext.Set<ExchangeRate>().Where(er => er.From == from && er.To == to).SingleOrDefaultAsync(cancellationToken);

    public ExchangeRate AddExchangeRate(ExchangeRate exchangeRate)
    {
        dataContext.Set<ExchangeRate>().Add(exchangeRate);
        return exchangeRate;
    }

    public async Task<IEnumerable<ExchangeRate>> GetExchangeRates(CancellationToken cancellationToken = default) =>
        await dataContext.Set<ExchangeRate>().ToListAsync(cancellationToken);
}
