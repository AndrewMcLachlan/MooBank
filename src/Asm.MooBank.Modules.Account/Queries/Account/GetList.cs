using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.Account;

public sealed record GetList() : IQuery<IEnumerable<ListItem<Guid>>>;

internal class GetListHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> institutionAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, AccountHolder accountHolder) : IQueryHandler<GetList, IEnumerable<ListItem<Guid>>>
{

    public async ValueTask<IEnumerable<ListItem<Guid>>> Handle(GetList request, CancellationToken cancellationToken = default)
    {
        var userId = accountHolder.Id;

        var institutionAccounts1 = await institutionAccounts
                                      .Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings
                                      .Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId))
                                      .Select(a => new ListItem<Guid> { Id = a.Id, Name = a.Name })
                                      .ToListAsync(cancellationToken);

        return institutionAccounts1.Union(stockHoldings1).OrderBy(a => a.Name);
    }
}
