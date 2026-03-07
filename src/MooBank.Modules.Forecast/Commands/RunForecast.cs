using System.ComponentModel;
using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Services;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("RunForecast")]
public record RunForecast(Guid PlanId) : ICommand<ForecastResult>;

internal class RunForecastHandler(
    IQueryable<DomainEntities.ForecastPlan> plans,
    IForecastEngine forecastEngine,
    ISecurity security) : ICommandHandler<RunForecast, ForecastResult>
{
    public async ValueTask<ForecastResult> Handle(RunForecast command, CancellationToken cancellationToken)
    {
        var plan = await plans
            .Apply(new ForecastPlanDetailsSpecification())
            .SingleAsync(p => p.Id == command.PlanId, cancellationToken);

        await security.AssertFamilyPermission(plan.FamilyId);

        return await forecastEngine.Calculate(plan, cancellationToken);
    }
}
