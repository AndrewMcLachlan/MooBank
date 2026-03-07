using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;

namespace Asm.MooBank.Modules.Forecast.Queries;

public record GetPlannedItem(Guid PlanId, Guid ItemId) : IQuery<PlannedItem>;

internal class GetPlannedItemHandler(IQueryable<Domain.Entities.Forecast.ForecastPlan> forecastPlans, User user) : IQueryHandler<GetPlannedItem, PlannedItem>
{
    public async ValueTask<PlannedItem> Handle(GetPlannedItem query, CancellationToken cancellationToken)
    {
        var plan = await forecastPlans.Include(p => p.PlannedItems).FirstOrDefaultAsync(p => p.Id == query.PlanId && p.FamilyId == user.FamilyId, cancellationToken);

        var planItem = plan?.PlannedItems.FirstOrDefault(pi => pi.Id == query.ItemId);

        return planItem?.ToModel() ?? throw new NotFoundException("Planned item not found");
    }
}
