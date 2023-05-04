using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Queries.Reports;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports;

internal class GetByTagReport : IQueryHandler<Models.Queries.Reports.GetByTagReport, ByTagReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurityRepository _securityRepository;

    public GetByTagReport(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public async Task<ByTagReport> Handle(Models.Queries.Reports.GetByTagReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await ReportQueryExtensions.WhereByReportQuery(_transactions.Include(t => t.TransactionTags), request).ToListAsync(cancellationToken);

        var tagValuesInterim = transactions
            .GroupBy(t => t.TransactionTags)
            .SelectMany(g => g.Key.Select(t =>
            new TagValue
            {
                TagId = t.TransactionTagId,
                TagName = t.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.Amount)),
            })).ToList();

        var tagValues = tagValuesInterim.GroupBy(t => new { t.TagId, t.TagName }).Select(g => new TagValue
        {
            TagId = g.Key.TagId,
            TagName = g.Key.TagName,
            GrossAmount = g.Sum(t => t.GrossAmount),
        }).ToList();

        if (request.TagId == null)
        {
            var tagLessAmount = await _transactions.WhereByReportQuery(request).Where(t => !t.TransactionTags.Any()).SumAsync(t => t.Amount, cancellationToken);
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
