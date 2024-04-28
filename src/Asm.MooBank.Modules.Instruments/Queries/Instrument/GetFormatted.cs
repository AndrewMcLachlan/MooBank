﻿using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Queries.Instrument;

public sealed record GetFormatted() : IQuery<InstrumentsList>;

internal class GetFormattedHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> institutionAccounts, IQueryable<Domain.Entities.StockHolding.StockHolding> stockHoldings, IQueryable<Domain.Entities.Asset.Asset> assets, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetFormatted, InstrumentsList>
{

    public async ValueTask<InstrumentsList> Handle(GetFormatted request, CancellationToken cancellationToken = default)
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

        var allGroups = institutionAccounts1.Select(g => g.GetGroup(userId)).Union(stockHoldings1.Select(g => g.GetGroup(userId))).Distinct(new IIdentifiableEqualityComparer<Domain.Entities.Group.Group, Guid>()!);

        var groups = allGroups.Where(ag => ag != null).Select(ag =>
        {
            IEnumerable<MooBank.Models.Instrument> matchingAccounts = [
                .. institutionAccounts1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. stockHoldings1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
                .. assets1.Where(a => a.GetGroup(userId)?.Id == ag!.Id).ToModel(currencyConverter),
            ];

            return new Group
            {
                Name = ag!.Name,
                Instruments = matchingAccounts,
                Total = ag.ShowPosition ? matchingAccounts.Sum(a => a.CurrentBalanceLocalCurrency) : null,
            };
        });

        Group otherAccounts =
            new()
            {
                Name = "Other Accounts",
                Instruments = [
                    .. institutionAccounts1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                    .. stockHoldings1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                    .. assets1.Where(a => a.GetGroup(userId) == null).ToModel(currencyConverter),
                ],
            };

        return new InstrumentsList
        {
            Groups = groups.Union(otherAccounts.Instruments.Any() ? [otherAccounts] : []),
            Total = groups.Sum(g => g.Total ?? 0),
        };
    }
}
