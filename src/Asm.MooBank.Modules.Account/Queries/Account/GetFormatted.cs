using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Queries.Account;

public sealed record GetFormatted() : IQuery<AccountsList>;

internal class GetFormattedHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> institutionAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetFormatted, AccountsList>
{

    public async ValueTask<AccountsList> Handle(GetFormatted request, CancellationToken cancellationToken = default)
    {
        var userId = user.Id;

        var institutionAccounts1 = await institutionAccounts.Include(a => a.VirtualInstruments)
                                                            .Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                            .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings.Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var assets1 = await assets.Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Where(a => a.Owners.Any(ah => ah.UserId == userId) ||
                                                  a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var allGroups = institutionAccounts1.Select(g => g.GetAccountGroup(userId)).Union(stockHoldings1.Select(g => g.GetAccountGroup(userId))).Distinct(new IIdentifiableEqualityComparer<Domain.Entities.Group.Group, Guid>()!);

        var groups = allGroups.Where(ag => ag != null).Select(ag =>
        {
            IEnumerable<Instrument> matchingAccounts = [
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
