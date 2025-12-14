using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("UpdatePlannedItem")]
public record UpdatePlannedItem(Guid PlanId, Guid ItemId, PlannedItem Item) : ICommand<PlannedItem>;

internal class UpdatePlannedItemHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<UpdatePlannedItem, PlannedItem>
{
    public async ValueTask<PlannedItem> Handle(UpdatePlannedItem request, CancellationToken cancellationToken)
    {
        var plan = await forecastRepository.Get(request.PlanId, new ForecastPlanDetailsSpecification(), cancellationToken);

        await security.AssertFamilyPermission(plan.FamilyId);

        var entity = plan.PlannedItems.SingleOrDefault(i => i.Id == request.ItemId)
            ?? throw new NotFoundException("Planned item not found");

        entity.Name = request.Item.Name;
        entity.ItemType = (DomainEntities.PlannedItemType)request.Item.ItemType;
        entity.Amount = request.Item.Amount;
        entity.TagId = request.Item.TagId;
        entity.VirtualInstrumentId = request.Item.VirtualInstrumentId;
        entity.IsIncluded = request.Item.IsIncluded;
        entity.DateMode = (DomainEntities.PlannedItemDateMode)request.Item.DateMode;
        entity.FixedDate = request.Item.FixedDate;
        entity.ScheduleFrequency = request.Item.ScheduleFrequency;
        entity.ScheduleAnchorDate = request.Item.ScheduleAnchorDate;
        entity.ScheduleInterval = request.Item.ScheduleInterval;
        entity.ScheduleDayOfMonth = request.Item.ScheduleDayOfMonth;
        entity.ScheduleEndDate = request.Item.ScheduleEndDate;
        entity.WindowStartDate = request.Item.WindowStartDate;
        entity.WindowEndDate = request.Item.WindowEndDate;
        entity.AllocationMode = request.Item.AllocationMode.HasValue ? (DomainEntities.AllocationMode)request.Item.AllocationMode : null;
        entity.Notes = request.Item.Notes;

        plan.UpdatedUtc = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
