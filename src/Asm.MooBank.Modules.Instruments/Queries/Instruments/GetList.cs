using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Queries.Instruments;

public sealed record GetList() : IQuery<IEnumerable<ListItem<Guid>>>;

internal class GetListHandler(IQueryable<Domain.Entities.Account.LogicalAccount> logicalAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, User user) : IQueryHandler<GetList, IEnumerable<ListItem<Guid>>>
{

    public async ValueTask<IEnumerable<ListItem<Guid>>> Handle(GetList request, CancellationToken cancellationToken = default)
    {
        var logicalAccounts1 = await logicalAccounts
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.Account.LogicalAccount>(user.Id, user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.StockHolding.StockHolding>(user.Id, user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        var assets1 = await assets
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.Asset.Asset>(user.Id, user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        return logicalAccounts1.Union(stockHoldings1).Union(assets1).OrderBy(a => a.Name);
    }
}
