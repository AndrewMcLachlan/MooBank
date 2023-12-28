using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.Repositories;

public class ReferenceDataRepository(MooBankContext dataContext) : IReferenceDataRepository
{
    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) =>
        await dataContext.ImporterTypes.ToListAsync(cancellationToken);

    public async Task<IEnumerable<StockPriceHistory>> GetStockPrices(string internationalSymbol, CancellationToken cancellationToken = default) =>
        await dataContext.Set<StockPriceHistory>().ToListAsync(cancellationToken);

    public Task<IEnumerable<StockPriceHistory>> GetStockPrices(string symbol, string exchange, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public StockPriceHistory AddStockPrice(StockPriceHistory stockPrice)
    {
        throw new NotImplementedException();
    }
}
