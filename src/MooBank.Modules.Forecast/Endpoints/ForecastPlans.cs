using System.Net;
using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Queries;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Forecast.Endpoints;

public class ForecastPlans : EndpointGroupBase
{
    public override string Path => "/forecast/plans";

    public override string? Tag => "Forecast";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetPlans, IEnumerable<ForecastPlan>>("/")
            .WithNames("Get All Forecast Plans");

        routeGroupBuilder.MapQuery<GetPlan, ForecastPlan>("/{id}")
            .WithNames("Get Forecast Plan")
            .RequireAuthorization(Policies.GetForecastPlanPolicy());

        routeGroupBuilder.MapPostCreate<CreatePlan, ForecastPlan>("/", "Get Forecast Plan".ToMachine(), (plan) => new { id = plan.Id }, CommandBinding.Parameters)
            .WithNames("Create Forecast Plan");

        routeGroupBuilder.MapPutCommand<UpdatePlan, ForecastPlan>("/{id}")
            .WithNames("Update Forecast Plan")
            .RequireAuthorization(Policies.GetForecastPlanPolicy());

        routeGroupBuilder.MapDelete<DeletePlan>("/{id}")
            .WithNames("Delete Forecast Plan")
            .RequireAuthorization(Policies.GetForecastPlanPolicy());

        routeGroupBuilder.MapPatchCommand<ArchivePlan, ForecastPlan>("/{id}/archive")
            .WithNames("Archive Forecast Plan")
            .RequireAuthorization(Policies.GetForecastPlanPolicy());

        routeGroupBuilder.MapCommand<RunForecast, ForecastResult>("/{planId}/run")
            .WithNames("Run Forecast")
            .RequireAuthorization(Policies.GetForecastPlanPolicy("planId"));
    }
}
