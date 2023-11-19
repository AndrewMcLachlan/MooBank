using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Reports;
using Asm.MooBank.Queries.Transactions;

namespace Asm.MooBank.Queries.Reports;

public record GetByTagReport : TypedReportQuery, IQuery<ByTagReport>
{
    public int? ParentTagId { get; init; } = null;
}

internal class GetByTagReportHandler(IQueryable<Transaction> transactions, ISecurity securityRepository) : IQueryHandler<GetByTagReport, ByTagReport>
{
    private readonly IQueryable<Transaction> _transactions = transactions;
    private readonly ISecurity _securityRepository = securityRepository;

    public async ValueTask<ByTagReport> Handle(GetByTagReport request, CancellationToken cancellationToken)
    {
        _securityRepository.AssertAccountPermission(request.AccountId);


        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var transactions = await _transactions.IncludeTags().WhereByReportQuery(request).ToListAsync(cancellationToken);

        var tagValuesInterim = transactions
            .GroupBy(t => t.Tags)
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

        if (request.ParentTagId == null)
        {
            var tagLessAmount = await _transactions.IncludeTags().WhereByReportQuery(request).Where(t => !t.Splits.SelectMany(ts => ts.Tags).Any()).SumAsync(t => t.Amount, cancellationToken);
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
