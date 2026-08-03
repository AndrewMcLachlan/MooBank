using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Forecast.Endpoints;

public class PlannedItems : EndpointGroupBase
{
    public override string Path => "/forecast/plans/{planId}/items";

    public override string? Tag => "Forecast";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetPlannedItem, PlannedItem>("/{itemId}")
            .WithNames("Get Planned Item");

        routeGroupBuilder.MapPostCreate<CreatePlannedItem, PlannedItem>("/", "Get Planned Item".ToMachine(), (command, item) => new { planId = command.PlanId, itemId = item.Id }, RequestBinding.Parameters)
            .WithNames("Create Planned Item");

        routeGroupBuilder.MapPutCommand<UpdatePlannedItem, PlannedItem>("/{itemId}", binding: RequestBinding.Parameters)
            .WithNames("Update Planned Item");

        routeGroupBuilder.MapDeleteCommand<DeletePlannedItem>("/{itemId}")
            .WithNames("Delete Planned Item");

        routeGroupBuilder.MapQuery<GetPaymentCandidates, IEnumerable<PaymentCandidate>>("/{itemId}/payment-candidates")
            .WithNames("Get Payment Candidates");

        routeGroupBuilder.MapPutCommand<SetPlannedItemPayments, PlannedItem>("/{itemId}/payments", binding: RequestBinding.Parameters)
            .WithNames("Set Planned Item Payments");
    }
}
