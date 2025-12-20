using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;

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
        entity.ItemType = request.Item.ItemType;
        entity.Amount = request.Item.Amount;
        entity.TagId = request.Item.TagId;
        entity.VirtualInstrumentId = request.Item.VirtualInstrumentId;
        entity.IsIncluded = request.Item.IsIncluded;
        entity.DateMode = request.Item.DateMode;
        entity.Notes = request.Item.Notes;

        // Update navigation properties based on DateMode
        // Clear existing configurations
        entity.FixedDate = null;
        entity.Schedule = null;
        entity.FlexibleWindow = null;

        switch (request.Item.DateMode)
        {
            case PlannedItemDateMode.FixedDate when request.Item.FixedDate.HasValue:
                entity.FixedDate = new PlannedItemFixedDate
                {
                    PlannedItemId = entity.Id,
                    FixedDate = request.Item.FixedDate.Value
                };
                break;

            case PlannedItemDateMode.Schedule when request.Item.ScheduleAnchorDate.HasValue:
                entity.Schedule = new PlannedItemSchedule
                {
                    PlannedItemId = entity.Id,
                    Frequency = request.Item.ScheduleFrequency ?? ScheduleFrequency.Monthly,
                    AnchorDate = request.Item.ScheduleAnchorDate.Value,
                    Interval = request.Item.ScheduleInterval ?? 1,
                    DayOfMonth = request.Item.ScheduleDayOfMonth,
                    EndDate = request.Item.ScheduleEndDate
                };
                break;

            case PlannedItemDateMode.FlexibleWindow when request.Item.WindowStartDate.HasValue && request.Item.WindowEndDate.HasValue:
                entity.FlexibleWindow = new PlannedItemFlexibleWindow
                {
                    PlannedItemId = entity.Id,
                    StartDate = request.Item.WindowStartDate.Value,
                    EndDate = request.Item.WindowEndDate.Value,
                    AllocationMode = request.Item.AllocationMode ?? AllocationMode.EvenlySpread
                };
                break;
        }

        plan.UpdatedUtc = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
