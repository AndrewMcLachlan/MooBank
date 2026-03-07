using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Queries;

public record GetPlan(Guid Id) : IQuery<Models.ForecastPlan>;

internal class GetPlanHandler(IQueryable<DomainEntities.ForecastPlan> plans, ISecurity security) : IQueryHandler<GetPlan, Models.ForecastPlan>
{
    public async ValueTask<Models.ForecastPlan> Handle(GetPlan query, CancellationToken cancellationToken)
    {
        var plan = await plans
            .Apply(new ForecastPlanDetailsSpecification())
            .SingleAsync(p => p.Id == query.Id, cancellationToken);

        await security.AssertFamilyPermission(plan.FamilyId);
        return plan.ToModel();
    }
}
