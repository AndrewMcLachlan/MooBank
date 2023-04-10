using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports;

internal class GetTagTrendReport : IQueryHandler<Models.Queries.Reports.GetTagTrendReport, TagTrendReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly IQueryable<TransactionTag> _tags;
    private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
    private readonly ISecurityRepository _securityRepository;

    public GetTagTrendReport(IQueryable<Transaction> transactions, IQueryable<TransactionTag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _tags = tags;
        _tagRelationships = tagRelationships;
        _securityRepository = securityRepository;
    }

    public async Task<TagTrendReport> Handle(Models.Queries.Reports.GetTagTrendReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);

        var transactionTypeFilter = request.ReportType.ToTransactionFilter();

        var tag = await _tags.SingleAsync(t => t.TransactionTagId == request.TagId, cancellationToken);
        var tags = await _tags.Include(t => t.Tags).Where(t => !t.Deleted && t.TaggedTo.Any(t2 => t2.TransactionTagId == request.TagId)).ToListAsync(cancellationToken);
        var tagHierarchies = await _tagRelationships.Include(t => t.TransactionTag).ThenInclude(t => t.Tags).Include(t => t.ParentTag).ThenInclude(t => t.Tags).Where(tr => tr.Ordinal == 1 && tags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
        var allTags = tags.Union(tagHierarchies.Select(t => t.TransactionTag)).ToList();
        allTags.Add(tag);

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.Include(t => t.TransactionTags).ThenInclude(t => t.Tags).Where(t => !t.ExcludeFromReporting && t.TransactionTime >= start && t.TransactionTime <= end && t.TransactionTags.Any(tt => allTags.Contains(tt))).Where(transactionTypeFilter).ToListAsync(cancellationToken);


        var months = transactions.GroupBy(t => new DateOnly(t.TransactionTime.Year, t.TransactionTime.Month, 1)).OrderBy(g => g.Key).Select(g => new TrendPoint
        {
            Month = g.Key,
            Amount = g.Sum(t => t.Amount)
        });

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            TagId = request.TagId,
            TagName = tag.Name,
            Months = months,
        };
    }
}
