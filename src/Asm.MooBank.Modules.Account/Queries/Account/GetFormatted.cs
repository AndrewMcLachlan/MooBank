using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.Account;

public sealed record GetFormatted() : IQuery<AccountsList>;

internal class GetFormattedHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> institutionAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, AccountHolder accountHolder, ICurrencyConverter currencyConverter) : IQueryHandler<GetFormatted, AccountsList>
{

    public async ValueTask<AccountsList> Handle(GetFormatted request, CancellationToken cancellationToken = default)
    {
        var userId = accountHolder.Id;

        var institutionAccounts1 = await institutionAccounts.Include(a => a.VirtualAccounts)
                                                            .Include(a => a.AccountHolders).ThenInclude(a => a.AccountGroup).Include(a => a.AccountHolders).ThenInclude(a => a.AccountHolder)
                                                            .Include(a => a.AccountViewers).ThenInclude(a => a.AccountGroup).Include(a => a.AccountViewers).ThenInclude(a => a.AccountHolder)
                                      .Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId))
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings.Include(a => a.AccountHolders).ThenInclude(a => a.AccountGroup).Include(a => a.AccountHolders).ThenInclude(a => a.AccountHolder)
                                                .Include(a => a.AccountViewers).ThenInclude(a => a.AccountGroup).Include(a => a.AccountViewers).ThenInclude(a => a.AccountHolder)
                                      .Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId))
                                      .ToListAsync(cancellationToken);

        var assets1 = await assets.Include(a => a.AccountHolders).ThenInclude(a => a.AccountGroup).Include(a => a.AccountHolders).ThenInclude(a => a.AccountHolder)
                                                .Include(a => a.AccountViewers).ThenInclude(a => a.AccountGroup).Include(a => a.AccountViewers).ThenInclude(a => a.AccountHolder)
                                      .Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId))
                                      .ToListAsync(cancellationToken);

        var allGroups = institutionAccounts1.Select(g => g.GetAccountGroup(userId)).Union(stockHoldings1.Select(g => g.GetAccountGroup(userId))).Distinct(new IIdentifiableEqualityComparer<Domain.Entities.Group.Group, Guid>()!);

        var groups = allGroups.Where(ag => ag != null).Select(ag =>
        {
            IEnumerable<Models.Account.Account> matchingAccounts = [
                .. institutionAccounts1.Where(a => a.GetAccountGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. stockHoldings1.Where(a => a.GetAccountGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. assets1.Where(a => a.GetAccountGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
            ];

            return new AccountListGroup
            {
                Name = ag!.Name,
                Accounts = matchingAccounts,
                Position = ag.ShowPosition ? matchingAccounts.Sum(a => a.CurrentBalanceLocalCurrency) : null,
            };
        });

        AccountListGroup otherAccounts =
            new()
            {
                Name = "Other Accounts",
                Accounts = [
                    .. institutionAccounts1.Where(a => a.GetAccountGroup(userId) == null).ToModel(currencyConverter),
                    .. stockHoldings1.Where(a => a.GetAccountGroup(userId) == null).ToModel(currencyConverter),
                    .. assets1.Where(a => a.GetAccountGroup(userId) == null).ToModel(currencyConverter),
                ],
            };

        return new AccountsList
        {
            AccountGroups = groups.Union(otherAccounts.Accounts.Any() ? [otherAccounts] : []),
            Position = groups.Sum(g => g.Position ?? 0),
        };
    }
}
