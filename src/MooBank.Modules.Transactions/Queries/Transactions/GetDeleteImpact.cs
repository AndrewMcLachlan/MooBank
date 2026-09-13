using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Modules.Transactions.Models;

namespace Asm.MooBank.Modules.Transactions.Queries.Transactions;

/// <summary>
/// What deleting a transaction would take with it.
/// </summary>
public record GetDeleteImpact(Guid InstrumentId, Guid Id) : IQuery<TransactionDeleteImpact>;

internal class GetDeleteImpactHandler(IQueryable<Transaction> transactions, IQueryable<ForecastPlan> plans) : IQueryHandler<GetDeleteImpact, TransactionDeleteImpact>
{
    public async ValueTask<TransactionDeleteImpact> Handle(GetDeleteImpact query, CancellationToken cancellationToken)
    {
        var refunds = await transactions
            .Where(t => t.Id == query.Id && t.AccountId == query.InstrumentId)
            .SelectMany(t => t.OffsetFor)
            .Select(o => new TransactionDeleteImpactRefund(o.Amount, o.TransactionSplit.Transaction.Description))
            .ToListAsync(cancellationToken);

        var plannedItems = await plans
            .SelectMany(p => p.PlannedItems
                .Where(i => i.Transactions.Any(t => t.TransactionId == query.Id))
                .Select(i => new TransactionDeleteImpactPlannedItem(p.Name, i.Name)))
            .ToListAsync(cancellationToken);

        return new TransactionDeleteImpact
        {
            Refunds = refunds,
            PlannedItems = plannedItems,
        };
    }
}
