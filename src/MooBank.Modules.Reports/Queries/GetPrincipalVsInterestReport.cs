using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetPrincipalVsInterestReport : ReportQuery, IQuery<PrincipalVsInterestReport>;

internal class GetPrincipalVsInterestReportHandler(
    IReportRepository repository,
    IQueryable<LogicalAccount> accounts,
    IQueryable<TagEntity> tags) : IQueryHandler<GetPrincipalVsInterestReport, PrincipalVsInterestReport>
{
    public async ValueTask<PrincipalVsInterestReport> Handle(GetPrincipalVsInterestReport request, CancellationToken cancellationToken)
    {
        var interestTagId = await accounts
            .Where(a => a.Id == request.AccountId)
            .SelectMany(a => a.TagPurposes)
            .Where(t => t.Purpose == TagPurpose.MortgageInterest)
            .Select(t => (int?)t.TagId)
            .FirstOrDefaultAsync(cancellationToken);

        if (interestTagId is null)
        {
            return new()
            {
                AccountId = request.AccountId,
                Start = request.Start,
                End = request.End,
                Interest = [],
                Principal = [],
                InterestTotal = 0m,
                PrincipalTotal = 0m,
            };
        }

        var monthlyTotals = await repository.GetMonthlyCreditDebitTotals(request.AccountId, request.Start, request.End, cancellationToken);
        var monthlyDebits = monthlyTotals
            .Where(t => t.TransactionType == TransactionFilterType.Debit)
            .ToDictionary(t => t.Month, t => t.Total);

        var interestTotals = await repository.GetMonthlyTotalsForTag(request.AccountId, request.Start, request.End, TransactionFilterType.Debit, interestTagId, cancellationToken);
        var interestSeries = interestTotals.ToModel().ToList();

        var principalSeries = interestSeries
            .Select(i => new TrendPoint
            {
                Month = i.Month,
                GrossAmount = (monthlyDebits.TryGetValue(i.Month, out var debit) ? debit : 0m) - i.GrossAmount,
            })
            .ToList();

        var tag = await tags.SingleAsync(t => t.Id == interestTagId, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            InterestTagId = interestTagId,
            InterestTagName = tag.Name,
            Interest = interestSeries,
            Principal = principalSeries,
            InterestTotal = interestSeries.Sum(p => p.GrossAmount),
            PrincipalTotal = principalSeries.Sum(p => p.GrossAmount),
        };
    }
}
