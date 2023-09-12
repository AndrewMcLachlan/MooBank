﻿using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public record GetTagTrendReport(int TagId) : TypedReportQuery, IQuery<TagTrendReport>
{
    public TagTrendReportSettings Settings { get; init; } = new TagTrendReportSettings();
}

public record TagTrendReportSettings(bool ApplySmoothing = false);


internal class GetTagTrendReportHandler : IQueryHandler<GetTagTrendReport, TagTrendReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<Tag> _tags;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
    private readonly ISecurity _securityRepository;

    public GetTagTrendReportHandler(IQueryable<Transaction> transactions, IQueryable<Tag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurity securityRepository)
    {
        _transactions = transactions;
        _tags = tags;
        _tagRelationships = tagRelationships;
        _securityRepository = securityRepository;
    }

    public async Task<TagTrendReport> Handle(GetTagTrendReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var transactionTypeFilter = request.ReportType.ToTransactionFilter();

        var tag = await _tags.SingleAsync(t => t.Id == request.TagId, cancellationToken);
        var tags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && t.TaggedTo.Any(t2 => t2.Id == request.TagId)).ToListAsync(cancellationToken);
        var tagHierarchies = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => tr.Ordinal == 1 && tags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        var allTags = tags.Union(tagHierarchies.Select(t => t.TransactionTag)).ToList();
        allTags.Add(tag);


        var transactions = await _transactions.Include(t => t.Tags).ThenInclude(t => t.Tags)
            .WhereByReportQuery(request).Where(t => t.Tags.Any(tt => allTags.Contains(tt)))
            .ToListAsync(cancellationToken);

        var months = transactions.GroupBy(t => new DateOnly(t.TransactionTime.Year, t.TransactionTime.Month, 1)).OrderBy(g => g.Key).Select(g => new TrendPoint
        {
            Month = g.Key,
            Amount = g.Where(transactionTypeFilter).Sum(t => t.Amount),
            OffsetAmount = g.Sum(t => t.Amount)
        });

        if (request.Settings.ApplySmoothing)
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

    private IEnumerable<TrendPoint> ApplySmoothing(IEnumerable<TrendPoint> months)
    {
        if (!months.Any()) yield break;

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

            decimal difference = month.Month.Month - current.Month;
            var averageAmount = month.Amount / difference;
            var averageOffset = month.OffsetAmount / difference;

            for (var i = 0; i < difference; i++)
            {
                yield return new TrendPoint
                {
                    Amount = averageAmount,
                    Month = current.AddMonths(i),
                    OffsetAmount = averageOffset,
                };
            }

            current = month.Month;
        }
    }
}
