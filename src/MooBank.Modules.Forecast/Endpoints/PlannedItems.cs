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

public class PlannedItems : EndpointGroupBase
{
    public override string Name => "Planned Items";

    public override string Path => "/forecast/plans/{planId}/items";

    public override string Tags => "Forecast";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetPlannedItem, PlannedItem>("/{itemId}")
            .WithNames("Get Planned Item");

        routeGroupBuilder.MapPost("/", async ([AsParameters] CreatePlannedItem command, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.Dispatch(command, cancellationToken);

            return Results.CreatedAtRoute("Get Planned Item".ToMachine(), new { planId = command.PlanId, itemId = result.Id }, result);
        })
            .WithNames("Create Planned Item")
            .Produces<PlannedItem>((int)HttpStatusCode.Created);

        routeGroupBuilder.MapPutCommand<UpdatePlannedItem, PlannedItem>("/{itemId}")
            .WithNames("Update Planned Item");

        routeGroupBuilder.MapDelete<DeletePlannedItem>("/{itemId}")
            .WithNames("Delete Planned Item");
    }
}
