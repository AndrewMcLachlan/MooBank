using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Queries;

public sealed record Get(Guid Id) : IQuery<StockHolding>;

internal class GetHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> accounts, User user, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<Get, StockHolding>
{
    public async ValueTask<StockHolding> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .SingleOrDefaultAsync(a => a.Id == query.Id, cancellationToken) ?? throw new NotFoundException();

        security.AssertInstrumentPermission(entity);

        var account = entity.ToModel(user.Id, currencyConverter);

        return account!;
    }
}
