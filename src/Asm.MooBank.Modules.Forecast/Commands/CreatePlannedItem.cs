using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("CreatePlannedItem")]
public record CreatePlannedItem(Guid PlanId, PlannedItem Item) : ICommand<PlannedItem>;

internal class CreatePlannedItemHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<CreatePlannedItem, PlannedItem>
{
    public async ValueTask<PlannedItem> Handle(CreatePlannedItem request, CancellationToken cancellationToken)
    {
        var plan = await forecastRepository.Get(request.PlanId, new ForecastPlanDetailsSpecification(), cancellationToken);

        await security.AssertFamilyPermission(plan.FamilyId);

        var entity = request.Item.ToDomain(request.PlanId);
        plan.AddPlannedItem(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
