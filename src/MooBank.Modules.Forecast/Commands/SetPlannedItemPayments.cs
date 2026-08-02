using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;

namespace Asm.MooBank.Modules.Forecast.Commands;

/// <summary>
/// Records which payments belong to a planned item, replacing whatever was linked before.
/// </summary>
[DisplayName("SetPlannedItemPayments")]
public record SetPlannedItemPayments(Guid PlanId, Guid ItemId, PlannedItemPayments Payments) : ICommand<PlannedItem>;

internal class SetPlannedItemPaymentsHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork) : ICommandHandler<SetPlannedItemPayments, PlannedItem>
{
    public async ValueTask<PlannedItem> Handle(SetPlannedItemPayments request, CancellationToken cancellationToken)
    {
        var plan = await forecastRepository.Get(request.PlanId, new ForecastPlanDetailsSpecification(), cancellationToken);

        plan.SetPlannedItemTransactions(request.ItemId, request.Payments.TransactionIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.PlannedItems.Single(i => i.Id == request.ItemId).ToModel();
    }
}
