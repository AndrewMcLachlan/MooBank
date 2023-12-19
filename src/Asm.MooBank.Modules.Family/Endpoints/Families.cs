using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Family.Commands;
using Asm.MooBank.Modules.Family.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Family.Endpoints;
internal class Families : EndpointGroupBase
{
    public override string Name => "Families";

    public override string Path => "/families";

    public override string Tags => "Families";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Family>>("/")
            .WithName("Get All Families")
            .Produces<IEnumerable<Models.Family>>();

        routeGroupBuilder.MapQuery<Get, Models.Family>("/{id}")
            .WithName("Get Family")
            .Produces<Models.Family>();

        routeGroupBuilder.MapPostCreate<Create, Models.Family>("/", "Get Family".ToMachine(), (i) => new { i.Id });
    }
}
