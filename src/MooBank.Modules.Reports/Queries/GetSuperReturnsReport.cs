using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetSuperReturnsReport : ReportQuery, IQuery<SuperReturnsReport>;

internal class GetSuperReturnsReportHandler(
    IReportRepository repository,
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

        var balances = (await repository.GetMonthlyBalances(request.AccountId, request.Start, request.End, cancellationToken))
            .ToDictionary(b => b.PeriodEnd, b => b.Balance);

        var years = new List<SuperReturnsYear>();

        foreach (var fy in FinancialYear.Range(request.Start, request.End))
        {
            var opening = FindOpeningBalance(balances, fy.Start);
            var closing = FindClosingBalance(balances, fy.End);

            var contributions = 0m;
            foreach (var tagId in configured)
            {
                var totals = await repository.GetMonthlyTotalsForTag(request.AccountId, fy.Start, fy.End, TransactionFilterType.Credit, tagId, cancellationToken);
                contributions += totals.Sum(t => t.GrossAmount);
            }

            var implied = closing - opening - contributions;
            decimal? returnPercent = opening != 0m ? Math.Round(implied / opening * 100m, 2) : null;

            years.Add(new SuperReturnsYear
            {
                FinancialYear = fy.Year,
                Start = fy.Start,
                End = fy.End,
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
            Start = request.Start,
            End = request.End,
            Years = years,
        };
    }

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
