using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("DeletePlannedItem")]
public record DeletePlannedItem(Guid PlanId, Guid ItemId) : ICommand;

internal class DeletePlannedItemHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork) : ICommandHandler<DeletePlannedItem>
{
    public async ValueTask Handle(DeletePlannedItem request, CancellationToken cancellationToken)
    {
        var plan = await forecastRepository.Get(request.PlanId, new ForecastPlanDetailsSpecification(), cancellationToken);

        plan.RemovePlannedItem(request.ItemId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
