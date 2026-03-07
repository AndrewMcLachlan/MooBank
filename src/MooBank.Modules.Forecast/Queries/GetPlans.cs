using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Queries;

public record GetPlans(bool IncludeArchived = false) : IQuery<IEnumerable<Models.ForecastPlan>>;

internal class GetPlansHandler(IQueryable<DomainEntities.ForecastPlan> plans, User user) : IQueryHandler<GetPlans, IEnumerable<Models.ForecastPlan>>
{
    public async ValueTask<IEnumerable<Models.ForecastPlan>> Handle(GetPlans query, CancellationToken cancellationToken)
    {
        var result = await plans
            .Where(p => p.FamilyId == user.FamilyId && (query.IncludeArchived || !p.IsArchived))
            .OrderByDescending(p => p.UpdatedUtc)
            .ToListAsync(cancellationToken);

        return result.Select(p => p.ToModel());
    }
}
