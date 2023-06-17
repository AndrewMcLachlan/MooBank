using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Queries.Budget;

public record GetValueForTag(Guid AccountId, int TagId) : IQuery<decimal>
{
    public DateOnly Start { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToStartOfMonth();
    public DateOnly End { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToEndOfMonth();
}

internal class GetValueForTagHandler : IQueryHandler<GetValueForTag, decimal>
{
    private readonly IQueryable<Transaction> _transactions;
    private readonly ISecurity _security;

    public GetValueForTagHandler(IQueryable<Transaction> transactions, ISecurity security)
    {
        _transactions = transactions;
        _security = security;
    }

    public async Task<decimal> Handle(GetValueForTag request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var query = _transactions.Where(t => t.AccountId == request.AccountId && !t.ExcludeFromReporting && t.TransactionTags.Any(tt => tt.TransactionTagId == request.TagId));

        var sum = await query.Where(t => t.TransactionTime >= start && t.TransactionTime <= end).Select(t => t.NetAmount).SumAsync(cancellationToken);

        if (sum != 0) return sum;

        return await query.OrderByDescending(t => t.TransactionTime).Select(t => t.NetAmount).FirstOrDefaultAsync(cancellationToken);

    }
}
