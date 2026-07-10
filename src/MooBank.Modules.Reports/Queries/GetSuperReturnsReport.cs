using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

/// <summary>
/// Annual returns are derived from the account's own balance history rather
/// than a user-selected period — the chart starts at the earliest FY for
/// which a return can be computed and ends no later than the current FY.
/// </summary>
public record GetSuperReturnsReport : IQuery<SuperReturnsReport>
{
    public required Guid AccountId { get; init; }
}

internal class GetSuperReturnsReportHandler(
    IReportReader reportReader,
    IQueryable<LogicalAccount> accounts) : IQueryHandler<GetSuperReturnsReport, SuperReturnsReport>
{
    public async ValueTask<SuperReturnsReport> Handle(GetSuperReturnsReport request, CancellationToken cancellationToken)
    {
        var configured = await accounts
            .Where(a => a.Id == request.AccountId)
            .SelectMany(a => a.TagPurposes)
            .Where(t => t.Purpose == TagPurpose.EmployerContribution || t.Purpose == TagPurpose.PersonalContribution)
            .Select(t => t.TagId)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rawBalances = (await reportReader.GetMonthlyBalances(request.AccountId, DateOnly.MinValue, today, cancellationToken)).ToList();

        if (rawBalances.Count == 0)
        {
            return Empty(request.AccountId, today);
        }

        var balances = rawBalances.ToDictionary(b => b.PeriodEnd, b => b.Balance);
        var earliest = rawBalances.Min(b => b.PeriodEnd);
        var latest = rawBalances.Max(b => b.PeriodEnd);

        // We need a prior balance to use as opening, so the first computable FY
        // is the one after the FY containing the earliest balance entry.
        var firstFy = FinancialYear.For(earliest).Year + 1;
        var lastFy = Math.Min(FinancialYear.For(latest).Year, FinancialYear.For(today).Year);

        if (firstFy > lastFy)
        {
            return Empty(request.AccountId, today);
        }

        var years = new List<SuperReturnsYear>();

        // Fetch contributions once per tag over the full reporting window and bucket the
        // monthly totals by financial year, rather than one reportReader call per FY per tag.
        var rangeStart = new DateOnly(firstFy - 1, 7, 1);
        var rangeEnd = new DateOnly(lastFy, 6, 30);
        var contributionsByFy = new Dictionary<int, decimal>();

        foreach (var tagId in configured)
        {
            var totals = await reportReader.GetMonthlyTotalsForTag(request.AccountId, rangeStart, rangeEnd, TransactionFilterType.Credit, tagId, cancellationToken);

            foreach (var total in totals)
            {
                var fy = FinancialYear.For(total.Month).Year;
                contributionsByFy[fy] = contributionsByFy.GetValueOrDefault(fy) + total.GrossAmount;
            }
        }

        for (var year = firstFy; year <= lastFy; year++)
        {
            var fyStart = new DateOnly(year - 1, 7, 1);
            var fyEnd = new DateOnly(year, 6, 30);

            var opening = FindOpeningBalance(balances, fyStart);
            var closing = FindClosingBalance(balances, fyEnd);

            var contributions = contributionsByFy.GetValueOrDefault(year);

            var implied = closing - opening - contributions;
            decimal? returnPercent = opening != 0m ? Math.Round(implied / opening * 100m, 2) : null;

            years.Add(new SuperReturnsYear
            {
                FinancialYear = year,
                Start = fyStart,
                End = fyEnd,
                OpeningBalance = opening,
                ClosingBalance = closing,
                Contributions = contributions,
                Return = implied,
                ReturnPercent = returnPercent,
            });
        }

        return new()
        {
            AccountId = request.AccountId,
            Start = years[0].Start,
            End = years[^1].End,
            Years = years,
        };
    }

    private static SuperReturnsReport Empty(Guid accountId, DateOnly today) => new()
    {
        AccountId = accountId,
        Start = today,
        End = today,
        Years = [],
    };

    private static decimal FindOpeningBalance(IReadOnlyDictionary<DateOnly, decimal> balances, DateOnly fyStart)
    {
        // GetMonthlyBalances returns end-of-month balances. The opening balance for the FY
        // is the last known balance prior to fyStart (so the 30 Jun close of the previous FY).
        var candidate = balances.Keys.Where(d => d < fyStart).DefaultIfEmpty().Max();
        return candidate == default ? 0m : balances[candidate];
    }

    private static decimal FindClosingBalance(IReadOnlyDictionary<DateOnly, decimal> balances, DateOnly fyEnd)
    {
        var candidate = balances.Keys.Where(d => d <= fyEnd).DefaultIfEmpty().Max();
        return candidate == default ? 0m : balances[candidate];
    }
}
