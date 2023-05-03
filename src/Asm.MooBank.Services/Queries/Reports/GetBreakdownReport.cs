using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Models.Queries.Reports;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports;

internal class GetBreakdownReport : IQueryHandler<Models.Queries.Reports.GetBreakdownReport, BreakdownReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<TransactionTag> _tags;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
    private readonly ISecurityRepository _securityRepository;

    public GetBreakdownReport(IQueryable<Transaction> transactions, IQueryable<TransactionTag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _tags = tags;
        _tagRelationships = tagRelationships;
        _securityRepository = securityRepository;
    }

    public async Task<BreakdownReport> Handle(Models.Queries.Reports.GetBreakdownReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        TransactionTag? rootTag = null;
        IList<TransactionTag> tags;
        List<TransactionTag> topTags;
        IEnumerable<TransactionTagRelationship> lowerTags;

        if (request.TagId == null)
        {
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && !t.TaggedTo.Any()).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);

        }
        else
        {
            // Include the root tag for any transactions
            rootTag = _tags.Include(t => t.Tags).Single(t => t.TransactionTagId == request.TagId);
            topTags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && t.TaggedTo.Any(t2 => t2.TransactionTagId == request.TagId)).ToListAsync(cancellationToken);
            lowerTags = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted && topTags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        }


        tags = topTags.Union(lowerTags.Select(tr => tr.TransactionTag), new TransactionTagEqualityComparer()).ToList();
        if (rootTag != null)
        {
            tags.Add(rootTag);
        }

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.Include(t => t.TransactionTags).ThenInclude(t => t.Tags).WhereByQuery(request).Where(t => t.TransactionTags.Any(tt => tags.Contains(tt))).ToListAsync(cancellationToken);

        var tagValues = transactions
            .GroupBy(t => rootTag != null && t.TransactionTags.Contains(rootTag) ? rootTag : (topTags.FirstOrDefault(tag => t.TransactionTags.Contains(tag)) ?? lowerTags.Where(tag => t.TransactionTags.Contains(tag.TransactionTag)).Select(tag => tag.ParentTag).First()))
            .Select(g => new TagValue
            {
                TagId = g.Key.TransactionTagId,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.Amount)),
                HasChildren = g.Key.Tags.Any(t => !t.Deleted),
            }).ToList();

        if (request.TagId == null)
        {
            var tagLessAmount = await _transactions.WhereByQuery(request).Where(t => !t.TransactionTags.Any()).SumAsync(t => t.Amount, cancellationToken);
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
