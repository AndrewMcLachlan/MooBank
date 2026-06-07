using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetSavingsInterestReport : ReportQuery, IQuery<SavingsInterestReport>
{
    public required int TagId { get; init; }
}

internal class GetSavingsInterestReportHandler(IReportRepository repository, IQueryable<TagEntity> tags) : IQueryHandler<GetSavingsInterestReport, SavingsInterestReport>
{
    public async ValueTask<SavingsInterestReport> Handle(GetSavingsInterestReport request, CancellationToken cancellationToken)
    {
        var tagTotals = await repository.GetMonthlyTotalsForTag(
            request.AccountId,
            request.Start,
            request.End,
            TransactionFilterType.Credit,
            request.TagId,
            cancellationToken);

        var months = tagTotals.ToModel().ToList();
        var tag = await tags.SingleAsync(t => t.Id == request.TagId, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            TagId = request.TagId,
            TagName = tag.Name,
            Months = months,
            Total = months.Sum(m => m.GrossAmount),
            MonthlyAverage = months.Average(),
        };
    }
}
