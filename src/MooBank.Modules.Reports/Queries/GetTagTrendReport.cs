using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetTagTrendReport : TypedReportQuery, IQuery<TagTrendReport>
{
    public int TagId { get; init; }

    public bool? ApplySmoothing { get; init; } = false;
}

internal class GetTagTrendReportHandler(IReportRepository repository, IQueryable<Tag> tags) : IQueryHandler<GetTagTrendReport, TagTrendReport>
{
    public async ValueTask<TagTrendReport> Handle(GetTagTrendReport request, CancellationToken cancellationToken)
    {
        var tagTotals = await repository.GetMonthlyTotalsForTag(request.AccountId, request.Start, request.End, request.ReportType, request.TagId, cancellationToken);

        var months = tagTotals.ToModel();

        var tag = await tags.SingleAsync(t => t.Id == request.TagId, cancellationToken);

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
        if (!months.Any()) yield break;

        var ordered = months.OrderBy(m => m.Month).ToList();
        var current = ordered[0].Month;

        for (int i = 1; i < ordered.Count; i++)
        {
            var previous = ordered[i - 1];
            var next = ordered[i];

            var gap = next.Month.DifferenceInMonths(previous.Month);

            if (gap == 1)
            {
                yield return previous;
                current = next.Month;
                continue;
            }

            var avgGross = next.GrossAmount / gap;
            var avgNet = next.NetAmount / gap;

            for (int j = 0; j < gap; j++)
            {
                yield return new TrendPoint
                {
                    Month = previous.Month.AddMonths(j),
                    GrossAmount = avgGross,
                    NetAmount = avgNet,
                };
            }

            current = next.Month;
        }

        // Final point if it wasn't part of smoothing
        if (ordered.Count == 1 || ordered[^1].Month.DifferenceInMonths(ordered[^2].Month) == 1)
            yield return ordered[^1];

        /*if (!months.Any()) yield break;

        DateOnly current = months.First().Month;

        yield return months.First();

        foreach (var month in months.Skip(1))
        {
            if (month.Month < current) throw new InvalidOperationException("The report data points are no ordered by month");

            if (month.Month.Month == current.Month + 1)
            {
                current = month.Month;
                yield return month;
                continue;
            }

            decimal difference = month.Month.DifferenceInMonths(current);
            var averageAmount = month.GrossAmount / difference;
            var averageOffset = month.NetAmount / difference;

            for (var i = 0; i < difference; i++)
            {
                yield return new TrendPoint
                {
                    GrossAmount = averageAmount,
                    Month = current.AddMonths(i),
                    NetAmount = averageOffset,
                };
            }

            current = month.Month;
        }*/
    }
}
