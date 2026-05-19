using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetUserSavingsBreakdown : UserReportQuery, IQuery<UserSavingsBreakdownReport>;

internal class GetUserSavingsBreakdownHandler(
    IQueryable<LogicalAccount> accounts,
    IQueryable<StockHolding> stockHoldings,
    IQueryable<Asset> assets,
    IReportRepository repository,
    IQueryDispatcher queryDispatcher,
    User user) : IQueryHandler<GetUserSavingsBreakdown, UserSavingsBreakdownReport>
{
    public async ValueTask<UserSavingsBreakdownReport> Handle(GetUserSavingsBreakdown request, CancellationToken cancellationToken)
    {
        var (start, end) = request.ResolveRange();

        var cashFlow = await queryDispatcher.Dispatch(new GetUserCashFlow { Start = start, End = end }, cancellationToken);

        var accessibleAccounts = await accounts.AccessibleTo(user).Select(a => new { a.Id, a.Name, a.AccountType }).ToListAsync(cancellationToken);
        var nonTransactional = accessibleAccounts.Where(a => !UserReportScope.TransactionalTypes.Contains(a.AccountType)).ToList();

        var instrumentChanges = new List<InstrumentBalanceChange>();

        if (nonTransactional.Count > 0)
        {
            var monthlyBalances = await repository.GetMonthlyBalancesForAccounts(nonTransactional.Select(a => a.Id), start, end, cancellationToken);

            foreach (var account in nonTransactional)
            {
                var series = monthlyBalances.TryGetValue(account.Id, out var rows) ? rows.OrderBy(b => b.PeriodEnd).ToList() : [];
                var startBalance = series.FirstOrDefault()?.Balance;
                var endBalance = series.LastOrDefault()?.Balance;
                var delta = (startBalance is not null && endBalance is not null) ? endBalance - startBalance : null;

                instrumentChanges.Add(new InstrumentBalanceChange
                {
                    InstrumentId = account.Id,
                    Name = account.Name,
                    AccountType = account.AccountType,
                    StartBalance = startBalance,
                    EndBalance = endBalance,
                    Delta = delta,
                });
            }
        }

        var stocks = await stockHoldings
            .Apply(new OpenAccessibleSpecification<StockHolding>(user.Id, user.FamilyId))
            .Select(s => new { s.Id, s.Name, s.CurrentValue })
            .ToListAsync(cancellationToken);

        foreach (var stock in stocks)
        {
            instrumentChanges.Add(new InstrumentBalanceChange
            {
                InstrumentId = stock.Id,
                Name = stock.Name,
                AccountType = null,
                EndBalance = stock.CurrentValue,
                Note = "Historical balance not tracked; only current value shown.",
            });
        }

        var assetsList = await assets
            .Apply(new OpenAccessibleSpecification<Asset>(user.Id, user.FamilyId))
            .Select(a => new { a.Id, a.Name, a.Value })
            .ToListAsync(cancellationToken);

        foreach (var asset in assetsList)
        {
            instrumentChanges.Add(new InstrumentBalanceChange
            {
                InstrumentId = asset.Id,
                Name = asset.Name,
                AccountType = null,
                EndBalance = asset.Value,
                Note = "Historical balance not tracked; only current value shown.",
            });
        }

        return new()
        {
            Start = start,
            End = end,
            CashFlow = cashFlow,
            InstrumentChanges = instrumentChanges,
        };
    }
}
