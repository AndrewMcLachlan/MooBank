using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Bills.Queries.Bills;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Bills.Endpoints;

internal class Bills : EndpointGroupBase
{
    public override string Name => "Bills";

    public override string Path => "/bills";

    public override string Tags => "Bills";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapPagedQuery<GetAll, Models.Bill>("/");
    }
}
