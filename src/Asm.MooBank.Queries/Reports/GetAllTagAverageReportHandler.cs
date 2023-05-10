using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Queries.Reports;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Queries.Reports;

internal class GetAllTagAverageReportHandler : IQueryHandler<Asm.MooBank.Queries.Reports.GetAllTagAverageReport, AllTagAverageReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
    private readonly ISecurity _securityRepository;

    public GetAllTagAverageReportHandler(IQueryable<Transaction> transactions, IQueryable<TransactionTagRelationship> tagRelationships, ISecurity securityRepository)
    {
        _transactions = transactions;
        _tagRelationships = tagRelationships;
        _securityRepository = securityRepository;
    }

    public async Task<AllTagAverageReport> Handle(Asm.MooBank.Queries.Reports.GetAllTagAverageReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        var relationships = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => !tr.TransactionTag.Deleted).ToListAsync(cancellationToken);


        var transactions = await ReportQueryExtensions.WhereByReportQuery(_transactions.Include(t => t.TransactionTags), request).ToListAsync(cancellationToken);

        var total = transactions.Sum(t => t.Amount);
        decimal months = Math.Max(transactions.Min(t => t.TransactionTime).DifferenceInMonths(transactions.Max(t => t.TransactionTime)), 1);

        var tagValuesInterim = transactions
            .GroupBy(t => t.TransactionTags)
            .SelectMany(g => g.Key.Select(t =>
            new TagValue
            {
                TagId = t.TransactionTagId,
                TagName = t.Name,
                GrossAmount = g.WhereByReportType(request.ReportType).Sum(t => t.Amount),
                NetAmount = g.Sum(t => t.Amount),
            }));

        var tagValues = tagValuesInterim.GroupBy(t => new { t.TagId, t.TagName }).Select(g => new TagValue
        {
            TagId = g.Key.TagId,
            TagName = g.Key.TagName,
            GrossAmount = g.Sum(t => t.GrossAmount),
            NetAmount = g.Sum(t => t.NetAmount),
        });

        var consolidatedTagValues = tagValues.Select(t =>
        {
            var relatedTags = relationships.Where(r => r.ParentId == t.TagId).Select(r => r.Id);

            return t with
            {
                GrossAmount = t.GrossAmount + tagValues.Where(tv => relatedTags.Contains(tv.TagId!.Value)).Sum(tv => tv.GrossAmount),
                NetAmount = t.NetAmount + tagValues.Where(tv => relatedTags.Contains(tv.TagId!.Value)).Sum(tv => tv.NetAmount),
                HasChildren = relatedTags.Any(),
            };
        });


        // Filter out small values
        var filteredTagValues = consolidatedTagValues.Where(t => (t.NetAmount / total) > 0.005m).Select(t => t with
        {
            NetAmount = Math.Round(Math.Abs(t.NetAmount!.Value / months)),
            GrossAmount = Math.Round(Math.Abs(t.GrossAmount / months)),
        });

        /*var tagLessAmount = await _transactions.Where(t => !t.ExcludeFromReporting && t.TransactionTime >= start && t.TransactionTime <= end && !t.TransactionTags.Any()).Where(transactionTypeFilter).SumAsync(t => t.Amount, cancellationToken);
        tagValues.Add(new TagValue
        {
            TagName = "Untagged",
            Amount = Math.Abs(tagLessAmount),
        });*/

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            ReportType = request.ReportType,
            Tags = filteredTagValues.Take(50).OrderByDescending(t => t.NetAmount),
        };
    }
}
