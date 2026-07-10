using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetTagTrendReport : TypedReportQuery, IQuery<TagTrendReport>
{
    public int TagId { get; init; }

    public bool? ApplySmoothing { get; init; } = false;
}

internal class GetTagTrendReportHandler(IReportRepository repository, IQueryable<Tag> tags, MooBank.Models.User user) : IQueryHandler<GetTagTrendReport, TagTrendReport>
{
    public async ValueTask<TagTrendReport> Handle(GetTagTrendReport request, CancellationToken cancellationToken)
    {
        var tagTotals = await repository.GetMonthlyTotalsForTag(request.AccountId, request.Start, request.End, request.ReportType, request.TagId, cancellationToken);

        var months = tagTotals.ToModel();

        // Trend reports remain available for soft-deleted tags; the tenant filter still applies.
        var tag = await tags.IgnoreQueryFilters(["SoftDelete"]).SingleAsync(t => t.Id == request.TagId && t.FamilyId == user.FamilyId, cancellationToken);

        if (request.ApplySmoothing ?? false)
        {
            months = ApplySmoothing(months);
        }

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            TagId = request.TagId,
            TagName = tag.Name,
            Months = months,
            Average = months.Average(),
            OffsetAverage = months.AverageOffset(),
        };
    }

    private static IEnumerable<TrendPoint> ApplySmoothing(IEnumerable<TrendPoint> months)
    {
        var ordered = months.OrderBy(m => m.Month).ToList();

        if (ordered.Count == 0) yield break;

        yield return ordered[0];

        for (int i = 1; i < ordered.Count; i++)
        {
            var previous = ordered[i - 1];
            var next = ordered[i];

            var gap = next.Month.DifferenceInMonths(previous.Month);

            if (gap <= 1)
            {
                yield return next;
                continue;
            }

            // Spread the next point's amount evenly over the months after the previous
            // point, up to and including the next point's own month. The previous point
            // keeps its own value and every month is emitted exactly once.
            var avgGross = next.GrossAmount / gap;
            var avgNet = next.NetAmount / gap;

            for (int j = 1; j <= gap; j++)
            {
                yield return new TrendPoint
                {
                    Month = previous.Month.AddMonths(j),
                    GrossAmount = avgGross,
                    NetAmount = avgNet,
                };
            }
        }
    }
}
