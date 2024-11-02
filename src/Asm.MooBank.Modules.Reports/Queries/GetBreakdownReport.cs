using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetBreakdownReport : TypedReportQuery, IQuery<BreakdownReport>
{
    public int? ParentTagId { get; init; } = null;
}

internal class GetBreakdownReportHandler(IQueryable<Transaction> transactions, IQueryable<Tag> tags, IQueryable<TagRelationship> tagRelationships, ISecurity security) : IQueryHandler<GetBreakdownReport, BreakdownReport>
{
    private readonly IQueryable<Transaction> _transactions = transactions;
    private readonly IQueryable<Tag> _tags = tags;

    public async ValueTask<BreakdownReport> Handle(GetBreakdownReport request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var parentTagId = request.ParentTagId;

        Tag? rootTag = null;
        List<Tag> topTags;
        IEnumerable<TagRelationship> lowerTags;

        if (parentTagId == null)
        {
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && t.TaggedTo.Count == 0).ToListAsync(cancellationToken);
            lowerTags = await tagRelationships.Include(t => t.Tag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.Tag.Deleted && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        }
        else
        {
            // Include the root tag for any transactions
            rootTag = _tags.Include(t => t.Tags).Single(t => t.Id == parentTagId);
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && t.TaggedTo.Any(t2 => t2.Id == parentTagId)).ToListAsync(cancellationToken);
            lowerTags = await tagRelationships.Include(t => t.Tag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.Tag.Deleted && (tr.Tag.Settings == null || !tr.Tag.Settings.ExcludeFromReporting) && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        }

        List<Tag> tags = topTags.Union(lowerTags.Select(tr => tr.Tag), new TagEqualityComparer()).ToList();
        if (rootTag != null)
        {
            tags.Add(rootTag);
        }

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.IncludeOffsets().IncludeTagsAndSubTags().WhereByReportQuery(request).Where(t => t.Splits.SelectMany(t => t.Tags).Any(tt => tags.Contains(tt))).ToListAsync(cancellationToken);

        var tagValues = transactions
            .GroupBy(t => rootTag != null && t.Tags.Contains(rootTag) ? rootTag : topTags.FirstOrDefault(tag => t.Tags.Contains(tag)) ?? lowerTags.Where(tag => t.Tags.Contains(tag.Tag)).Select(tag => tag.ParentTag).First())
            .Select(g => new TagValue
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.Amount)),
                NetAmount = Math.Abs(g.Sum(t => t.GetNetAmount())),
                HasChildren = g.Key.Tags.Any(t => !t.Deleted),
            }).ToList();

        // Only get amounts for transaction without tags if we are at the top level.
        if (parentTagId == null)
        {
            var tagLessAmount = await _transactions.IncludeTags().WhereByReportQuery(request).Where(t => !t.Splits.SelectMany(t => t.Tags).Any()).SumAsync(t => t.Amount, cancellationToken);
            var talLessNetAmount = await _transactions.IncludeTags().WhereByReportQuery(request).Where(t => !t.Splits.SelectMany(t => t.Tags).Any()).SumAsync(t => Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount), cancellationToken);
            tagValues.Add(new TagValue
            {
                TagName = "Untagged",
                GrossAmount = Math.Abs(tagLessAmount),
            });
        }

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            Tags = tagValues.OrderByDescending(t => t.GrossAmount),
        };
    }
}
