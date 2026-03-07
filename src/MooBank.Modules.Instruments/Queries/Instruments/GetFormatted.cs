using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Queries.Instruments;

public sealed record GetFormatted() : IQuery<InstrumentsList>;

internal class GetFormattedHandler(IQueryable<Domain.Entities.Account.LogicalAccount> logicalAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetFormatted, InstrumentsList>
{

    public async ValueTask<InstrumentsList> Handle(GetFormatted request, CancellationToken cancellationToken = default)
    {
        var userId = user.Id;

        var logicalAccounts1 = await logicalAccounts.Include(a => a.VirtualInstruments)
                                                            .Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                            .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.Account.LogicalAccount>(user.Id, user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var stockHoldings1 = await stockHoldings.Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.StockHolding.StockHolding>(user.Id, user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var assets1 = await assets.Include(a => a.Owners).ThenInclude(a => a.Group).Include(a => a.Owners).ThenInclude(a => a.User)
                                                .Include(a => a.Viewers).ThenInclude(a => a.Group).Include(a => a.Viewers).ThenInclude(a => a.User)
                                      .Apply(new OpenAccessibleSpecification<Domain.Entities.Asset.Asset>(user.Id, user.FamilyId))
                                      .ToListAsync(cancellationToken);

        var allGroups = logicalAccounts1.Select(g => g.GetGroup(userId))
            .Union(stockHoldings1.Select(g => g.GetGroup(userId)))
            .Union(assets1.Select(a => a.GetGroup(userId)))
            .Distinct(new IIdentifiableEqualityComparer<Domain.Entities.Group.Group, Guid>()!);

        var groups = allGroups.Where(ag => ag != null).Select(ag =>
        {
            IEnumerable<Instrument> matchingAccounts = [
                .. logicalAccounts1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. stockHoldings1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. assets1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
            ];

            return new Group
            {
                Id = ag!.Id,
                Name = ag!.Name,
                Instruments = matchingAccounts,
                ShowTotal = ag.ShowPosition,
                Total = matchingAccounts.Sum(a => a.CurrentBalanceLocalCurrency),
            };
        });

        Group otherAccounts =
            new()
            {
                Id = null,
                Name = "Other Accounts",
                Instruments = [
                    .. logicalAccounts1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                    .. stockHoldings1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                    .. assets1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                ],
            };

        otherAccounts.Total = otherAccounts.Instruments.Sum(a => a.CurrentBalanceLocalCurrency);

        return new InstrumentsList
        {
            Groups = groups.Union(otherAccounts.Instruments.Any() ? [otherAccounts] : []),
            Total = groups.Sum(g => g.Total ?? 0),
        };
    }
}
