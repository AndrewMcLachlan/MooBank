//using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetAllTagAverageReport() : TypedReportQuery, IQuery<AllTagAverageReport>
{
    public int Top { get; init; }
}

internal class GetAllTagAverageReportHandler(IQueryable<Transaction> transactions/*, IQueryable<TagRelationship> tagRelationships*/) : IQueryHandler<GetAllTagAverageReport, AllTagAverageReport>
{
    private readonly IQueryable<Transaction> _transactions = transactions;

    public async ValueTask<AllTagAverageReport> Handle(GetAllTagAverageReport query, CancellationToken cancellationToken)
    {
        // Only required by consolidated tag amounts below
        //var relationships = await tagRelationships.Include(t => t.Tag).ThenInclude(t => t.Tags.Where(t => !t.Deleted && !t.Settings.ExcludeFromReporting)).Include(t => t.ParentTag).ThenInclude(t => t.Tags.Where(t => !t.Deleted && !t.Settings.ExcludeFromReporting)).Where(tr => !tr.Tag.Deleted && !tr.Tag.Settings.ExcludeFromReporting).ToListAsync(cancellationToken);

        var transactions = await _transactions.IncludeOffsets().Include(t => t.Splits).ThenInclude(t => t.Tags.Where(t => !t.Deleted && !t.Settings.ExcludeFromReporting)).WhereByReportQuery(query).WhereByReportType(query.ReportType).ToListAsync(cancellationToken);

        if (transactions.Count == 0) return new()
        {
            AccountId = query.AccountId,
            Start = query.Start,
            End = query.End,
            ReportType = query.ReportType,
            Tags = [],
        };

        var total = transactions.Sum(t => t.GetNetAmount());
        decimal months = Math.Max(transactions.Min(t => t.TransactionTime).DifferenceInMonths(transactions.Max(t => t.TransactionTime)), 1);

        var tagValuesInterim = transactions
            .GroupBy(t => t.Tags)
            .SelectMany(g => g.Key.Distinct().Select(t =>
            new TagValue
            {
                TagId = t.Id,
                TagName = t.Name,
                GrossAmount = g.WhereByReportType(query.ReportType).Sum(t => t.Amount),
                NetAmount = g.Sum(t => t.GetNetAmount()),
            }));

        var tagValues = tagValuesInterim.GroupBy(t => new { t.TagId, t.TagName }).Select(g => new TagValue
        {
            TagId = g.Key.TagId,
            TagName = g.Key.TagName,
            GrossAmount = g.Sum(t => t.GrossAmount),
            NetAmount = g.Sum(t => t.NetAmount),
        });

        // This consolidates tag amounts by child values. However, it doesn't work as expected.
        // If you have no amounts tagged as "Medical", it doesn't appear
        // If you have 1 amount tagged as "Medical", it will show the all amounts under medical which is confusing.
        /*var consolidatedTagValues = tagValues.Select(t =>
        {
            var relatedTags = relationships.Where(r => r.ParentId == t.TagId).Select(r => r.Id);

            return t with
            {
                GrossAmount = t.GrossAmount + tagValues.Where(tv => relatedTags.Contains(tv.TagId!.Value)).Sum(tv => tv.GrossAmount),
                NetAmount = t.NetAmount + tagValues.Where(tv => relatedTags.Contains(tv.TagId!.Value)).Sum(tv => tv.NetAmount),
                HasChildren = relatedTags.Any(),
            };
        });*/


        // Filter out small values
        var filteredTagValues = tagValues.Where(t => t.NetAmount / total > 0.005m).Select(t => t with
        {
            NetAmount = Math.Round(Math.Abs(t.NetAmount!.Value / months)),
            GrossAmount = Math.Round(Math.Abs(t.GrossAmount / months)),
        });

        // Do not include amounts without tags
        /*var tagLessAmount = await _transactions.Where(t => !t.ExcludeFromReporting && t.TransactionTime >= start && t.TransactionTime <= end && !t.TransactionTags.Any()).Where(transactionTypeFilter).SumAsync(t => t.Amount, cancellationToken);
        tagValues.Add(new TagValue
        {
            TagName = "Untagged",
            Amount = Math.Abs(tagLessAmount),
        });*/

        return new()
        {
            AccountId = query.AccountId,
            Start = query.Start,
            End = query.End,
            ReportType = query.ReportType,
            Tags = filteredTagValues.OrderByDescending(t => t.NetAmount).Take(query.Top),
        };
    }
}
