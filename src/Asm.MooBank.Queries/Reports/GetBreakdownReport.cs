using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public record GetBreakdownReport(int? TagId) : TypedReportQuery, IQuery<BreakdownReport>;

internal class GetBreakdownReportHandler : IQueryHandler<GetBreakdownReport, BreakdownReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<Tag> _tags;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
    private readonly ISecurity _securityRepository;

    public GetBreakdownReportHandler(IQueryable<Transaction> transactions, IQueryable<Tag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurity securityRepository)
    {
        _transactions = transactions;
        _tags = tags;
        _tagRelationships = tagRelationships;
        _securityRepository = securityRepository;
    }

    public async Task<BreakdownReport> Handle(GetBreakdownReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        Tag? rootTag = null;
        IList<Tag> tags;
        List<Tag> topTags;
        IEnumerable<TransactionTagRelationship> lowerTags;

        if (request.TagId == null)
        {
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && !t.TaggedTo.Any()).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);

        }
        else
        {
            // Include the root tag for any transactions
            rootTag = _tags.Include(t => t.Tags).Single(t => t.Id == request.TagId);
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting) && t.TaggedTo.Any(t2 => t2.Id == request.TagId)).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && (tr.TransactionTag.Settings == null || !tr.TransactionTag.Settings.ExcludeFromReporting) && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        }


        tags = topTags.Union(lowerTags.Select(tr => tr.TransactionTag), new TagEqualityComparer()).ToList();
        if (rootTag != null)
        {
            tags.Add(rootTag);
        }

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.Include(t => t.TransactionSplits).ThenInclude(t => t.Tags).ThenInclude(t => t.Tags).WhereByReportQuery(request).Where(t => t.Tags.Any(tt => tags.Contains(tt))).ToListAsync(cancellationToken);

        var tagValues = transactions
            .GroupBy(t => rootTag != null && t.Tags.Contains(rootTag) ? rootTag : (topTags.FirstOrDefault(tag => t.Tags.Contains(tag)) ?? lowerTags.Where(tag => t.Tags.Contains(tag.TransactionTag)).Select(tag => tag.ParentTag).First()))
            .Select(g => new TagValue
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.Amount)),
                HasChildren = g.Key.Tags.Any(t => !t.Deleted),
            }).ToList();

        if (request.TagId == null)
        {
            var tagLessAmount = await _transactions.WhereByReportQuery(request).Where(t => !t.TransactionSplits.SelectMany(ts => ts.Tags).Any()).SumAsync(t => t.Amount, cancellationToken);
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
