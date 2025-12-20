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

        routeGroupBuilder.MapPostCreate<CreatePlannedItem, PlannedItem>("/", "Get Planned Item".ToMachine(), (item) => new { planId = Guid.Empty, itemId = item.Id }, CommandBinding.Parameters)
            .WithNames("Create Planned Item");

        routeGroupBuilder.MapPutCommand<UpdatePlannedItem, PlannedItem>("/{itemId}")
            .WithNames("Update Planned Item");

        routeGroupBuilder.MapDelete<DeletePlannedItem>("/{itemId}")
            .WithNames("Delete Planned Item");
    }
}
