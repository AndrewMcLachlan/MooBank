using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Queries;

public sealed record Get(Guid InstrumentId) : IQuery<StockHolding>;

internal class GetHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> accounts, User user, ICurrencyConverter currencyConverter, IReferenceDataRepository referenceDataRepository) : IQueryHandler<Get, StockHolding>
{
    public async ValueTask<StockHolding> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .SingleOrDefaultAsync(a => a.Id == query.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        var prices = await referenceDataRepository.GetStockPrices(entity.Symbol, cancellationToken);
        var previous = prices.OrderByDescending(p => p.Date).Skip(1).FirstOrDefault();

        var account = entity.ToModel(user.Id, currencyConverter) with
        {
            PreviousPrice = previous?.Price,
            PreviousPriceDate = previous?.Date,
        };

        return account;
    }
}
