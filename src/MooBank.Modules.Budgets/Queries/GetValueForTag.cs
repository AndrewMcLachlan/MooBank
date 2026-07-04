using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record GetValueForTag(int TagId) : IQuery<decimal>
{
    public DateOnly Start { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToStartOfMonth();
    public DateOnly End { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToEndOfMonth();
}

internal class GetValueForTagHandler(IQueryable<Transaction> transactions, MooBank.Models.User user) : IQueryHandler<GetValueForTag, decimal>
{
    public async ValueTask<decimal> Handle(GetValueForTag request, CancellationToken cancellationToken)
    {
        var familyId = user.FamilyId;
        var accountIds = user.Accounts;

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        // Exclude transactions whose net is zero (fully offset, e.g. a refund linked to its
        // purchase, or a transfer that nets out) so they don't distort the suggested amount,
        // and sum the net-of-offsets amount rather than the gross (mirrors the budget report).
        var query = transactions.Where(t => accountIds.Contains(t.AccountId) && !t.ExcludeFromReporting &&
                                            Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount) != 0m &&
                                            t.Splits.SelectMany(t => t.Tags).Any(tt => tt.Id == request.TagId));

        var sum = await query.Where(t => t.TransactionTime >= start && t.TransactionTime <= end)
            .Select(t => Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount)).SumAsync(cancellationToken);

        if (sum != 0) return sum;

        return await query.OrderByDescending(t => t.TransactionTime)
            .Select(t => Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount)).FirstOrDefaultAsync(cancellationToken);

    }
}
