using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetSavingsInterestReport : ReportQuery, IQuery<SavingsInterestReport>;

internal class GetSavingsInterestReportHandler(
    IReportRepository repository,
    IQueryable<LogicalAccount> accounts,
    IQueryable<TagEntity> tags) : IQueryHandler<GetSavingsInterestReport, SavingsInterestReport>
{
    public async ValueTask<SavingsInterestReport> Handle(GetSavingsInterestReport request, CancellationToken cancellationToken)
    {
        var tagId = await accounts
            .Where(a => a.Id == request.AccountId)
            .SelectMany(a => a.TagPurposes)
            .Where(t => t.Purpose == TagPurpose.Interest)
            .Select(t => (int?)t.TagId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tagId is null)
        {
            return new()
            {
                AccountId = request.AccountId,
                Start = request.Start,
                End = request.End,
                TagId = null,
                TagName = null,
                Months = [],
                Total = 0m,
                MonthlyAverage = 0m,
            };
        }

        var tagTotals = await repository.GetMonthlyTotalsForTag(
            request.AccountId,
            request.Start,
            request.End,
            TransactionFilterType.Credit,
            tagId,
            cancellationToken);

        var months = tagTotals.ToModel().ToList();
        var tag = await tags.SingleAsync(t => t.Id == tagId, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            TagId = tagId,
            TagName = tag.Name,
            Months = months,
            Total = months.Sum(m => m.GrossAmount),
            MonthlyAverage = months.Average(),
        };
    }
}
