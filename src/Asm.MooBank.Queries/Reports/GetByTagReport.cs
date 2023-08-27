using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Queries.Reports;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Queries.Reports;

public record GetByTagReport(int? TagId = null) : TypedReportQuery, IQuery<ByTagReport>;

internal class GetByTagReportHandler : IQueryHandler<GetByTagReport, ByTagReport>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurity _securityRepository;

    public GetByTagReportHandler(IQueryable<Transaction> transactions, ISecurity securityRepository)
    {
        _transactions = transactions;
        _securityRepository = securityRepository;
    }

    public async Task<ByTagReport> Handle(GetByTagReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.Include(t => t.TransactionTags).WhereByReportQuery(request).ToListAsync(cancellationToken);

        var tagValuesInterim = transactions
            .GroupBy(t => t.TransactionTags)
            .SelectMany(g => g.Key.Select(t =>
            new TagValue
            {
                TagId = t.Id,
                TagName = t.Name,
                GrossAmount = Math.Abs(g.Sum(t => t.NetAmount)), // This looks weird, but is correct
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
