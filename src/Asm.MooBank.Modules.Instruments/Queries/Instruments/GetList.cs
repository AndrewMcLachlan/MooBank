using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Queries.Instruments;

public sealed record GetList() : IQuery<IEnumerable<ListItem<Guid>>>;

internal class GetListHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> institutionAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, User user) : IQueryHandler<GetList, IEnumerable<ListItem<Guid>>>
{

    public async ValueTask<IEnumerable<ListItem<Guid>>> Handle(GetList request, CancellationToken cancellationToken = default)
    {
        var userId = user.Id;

        var institutionAccounts1 = await institutionAccounts
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        var assets1 = await assets
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        return institutionAccounts1.Union(stockHoldings1).Union(assets1).OrderBy(a => a.Name);
    }
}
