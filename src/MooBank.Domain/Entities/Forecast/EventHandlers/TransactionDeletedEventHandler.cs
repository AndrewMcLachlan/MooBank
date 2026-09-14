using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Domain.Entities.Transactions.Events;

namespace Asm.MooBank.Domain.Entities.Forecast.EventHandlers;

/// <summary>
/// Unlinks a deleted payment from the planned items that claimed it.
/// </summary>
/// <remarks>
/// The link has no cascade of its own, so without this the delete fails on the foreign key.
/// </remarks>
internal class TransactionDeletedEventHandler(IForecastRepository forecastRepository) : IDomainEventHandler<TransactionDeletedEvent>
{
    public async ValueTask Handle(TransactionDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var plans = await forecastRepository.Get(new ForecastPlansWithTransactionSpecification(domainEvent.TransactionId), cancellationToken);

        foreach (var plan in plans)
        {
            plan.RemoveTransactionLinks(domainEvent.TransactionId);
        }
    }
}
