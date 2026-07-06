using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetByTagReport : TypedReportQuery, IQuery<ByTagReport>
{
    public int? ParentTagId { get; init; } = null;
}

internal class GetByTagReportHandler(IQueryable<Transaction> transactions, IQueryable<TagRelationship> tagRelationships) : IQueryHandler<GetByTagReport, ByTagReport>
{
    public async ValueTask<ByTagReport> Handle(GetByTagReport request, CancellationToken cancellationToken)
    {
        var loaded = await transactions.Specify(new IncludeSplitsSpecification()).WhereByReportQuery(request).ToListAsync(cancellationToken);

        // Attribute each split's net amount to that split's tags, so a transaction split
        // across multiple tags only contributes each split's amount to the matching tag.
        var perTagAmounts = loaded
            .SelectMany(t => t.Splits.SelectMany(s => s.Tags.Select(tag =>
                (Tag: tag, Amount: t.TransactionType == TransactionType.Debit ? -s.GetNetAmount() : s.GetNetAmount()))));

        if (request.ParentTagId != null)
        {
            // Restrict the report to the parent tag and its descendants. The TagRelationship
            // view contains the transitive closure, not just direct parent/child pairs.
            var descendantIds = await tagRelationships
                .Where(r => r.ParentId == request.ParentTagId)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            var includedTagIds = descendantIds.Append(request.ParentTagId.Value).ToHashSet();

            perTagAmounts = perTagAmounts.Where(ta => includedTagIds.Contains(ta.Tag.Id));
        }

        var tagValues = perTagAmounts
            .GroupBy(ta => new { ta.Tag.Id, ta.Tag.Name })
            .Select(g => new TagValue
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(ta => ta.Amount)),
            })
            .ToList();

        if (request.ParentTagId == null)
        {
            var tagLessAmount = loaded.Where(t => !t.Splits.SelectMany(ts => ts.Tags).Any()).Sum(t => t.Amount);
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
