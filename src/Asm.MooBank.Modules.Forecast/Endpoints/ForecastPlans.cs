using System.Net;
using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Forecast.Endpoints;

public class ForecastPlans : EndpointGroupBase
{
    public override string Name => "Forecast Plans";

    public override string Path => "/forecast/plans";

    public override string Tags => "Forecast";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetPlans, IEnumerable<ForecastPlan>>("/")
            .WithNames("Get All Forecast Plans")
            .Produces<IEnumerable<ForecastPlan>>();

        routeGroupBuilder.MapQuery<GetPlan, ForecastPlan>("/{id}")
            .WithNames("Get Forecast Plan")
            .Produces<ForecastPlan>();

        routeGroupBuilder.MapPostCreate<CreatePlan, ForecastPlan>("/", "Get Forecast Plan".ToMachine(), (plan) => new { id = plan.Id }, CommandBinding.Parameters)
            .WithNames("Create Forecast Plan")
            .Produces<ForecastPlan>();

        routeGroupBuilder.MapPutCommand<UpdatePlan, ForecastPlan>("/{id}")
            .WithNames("Update Forecast Plan")
            .Produces<ForecastPlan>();

        routeGroupBuilder.MapDelete<DeletePlan>("/{id}")
            .WithNames("Delete Forecast Plan")
            .Produces((int)HttpStatusCode.NoContent);

        routeGroupBuilder.MapPatchCommand<ArchivePlan, ForecastPlan>("/{id}/archive")
            .WithNames("Archive Forecast Plan")
            .Produces<ForecastPlan>();

        routeGroupBuilder.MapCommand<RunForecast, ForecastResult>("/{planId}/run")
            .WithNames("Run Forecast")
            .Produces<ForecastResult>();
    }
}
