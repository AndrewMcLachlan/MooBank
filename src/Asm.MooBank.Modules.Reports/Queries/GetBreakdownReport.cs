using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models.Reports;
using Asm.MooBank.Queries.Transactions;

namespace Asm.MooBank.Queries.Reports;

public record GetBreakdownReport : TypedReportQuery, IQuery<BreakdownReport>
{
    public int? ParentTagId { get; init; } = null;
}

internal class GetBreakdownReportHandler(IQueryable<Transaction> transactions, IQueryable<Tag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurity securityRepository) : IQueryHandler<GetBreakdownReport, BreakdownReport>
{
    private readonly IQueryable<Transaction> _transactions = transactions;
    private readonly IQueryable<Tag> _tags = tags;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships = tagRelationships;
    private readonly ISecurity _securityRepository = securityRepository;

    public async ValueTask<BreakdownReport> Handle(GetBreakdownReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var parentTagId = request.ParentTagId;

        Tag? rootTag = null;
        List<Tag> topTags;
        IEnumerable<TransactionTagRelationship> lowerTags;

        if (parentTagId == null)
        {
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && !t.TaggedTo.Any()).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);

        }
        else
        {
            // Include the root tag for any transactions
            rootTag = _tags.Include(t => t.Tags).Single(t => t.Id == parentTagId);
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && t.TaggedTo.Any(t2 => t2.Id == parentTagId)).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && (tr.TransactionTag.Settings == null || !tr.TransactionTag.Settings.ExcludeFromReporting) && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        }


        List<Tag> tags = topTags.Union(lowerTags.Select(tr => tr.TransactionTag), new TagEqualityComparer()).ToList();
        if (rootTag != null)
        {
            tags.Add(rootTag);
        }

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.IncludeTagsAndSubTags().WhereByReportQuery(request).Where(t => t.Splits.SelectMany(t => t.Tags).Any(tt => tags.Contains(tt))).ToListAsync(cancellationToken);

        var tagValues = transactions
            .GroupBy(t => rootTag != null && t.Tags.Contains(rootTag) ? rootTag : (topTags.FirstOrDefault(tag => t.Tags.Contains(tag)) ?? lowerTags.Where(tag => t.Tags.Contains(tag.TransactionTag)).Select(tag => tag.ParentTag).First()))
            .Select(g => new TagValue
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.Amount)),
                HasChildren = g.Key.Tags.Any(t => !t.Deleted),
            }).ToList();

        if (parentTagId == null)
        {
            var tagLessAmount = await _transactions.IncludeTags().WhereByReportQuery(request).Where(t => !t.Splits.SelectMany(t => t.Tags).Any()).SumAsync(t => t.Amount, cancellationToken);
            tagValues.Add(new TagValue {
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
