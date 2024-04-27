using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Budget.Queries;

public record GetValueForTag(int TagId) : IQuery<decimal>
{
    public DateOnly Start { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToStartOfMonth();
    public DateOnly End { get; init; } = DateTime.Today.ToDateOnly().AddMonths(-1).ToEndOfMonth();
}

internal class GetValueForTagHandler(IQueryable<Transaction> transactions, MooBank.Models.User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetValueForTag, decimal>
{
    public async ValueTask<decimal> Handle(GetValueForTag request, CancellationToken cancellationToken)
    {
        var familyId = AccountHolder.FamilyId;
        var accountIds = AccountHolder.Accounts;

        var start = request.Start.ToStartOfDay();
        var end = request.End.ToEndOfDay();

        var query = transactions.Where(t => accountIds.Contains(t.AccountId) && !t.ExcludeFromReporting && t.Splits.SelectMany(t => t.Tags).Any(tt => tt.Id == request.TagId));

        var sum = await query.Where(t => t.TransactionTime >= start && t.TransactionTime <= end).Select(t => t.NetAmount).SumAsync(cancellationToken);

        if (sum != 0) return sum;

        return await query.OrderByDescending(t => t.TransactionTime).Select(t => t.NetAmount).FirstOrDefaultAsync(cancellationToken);

    }
}
