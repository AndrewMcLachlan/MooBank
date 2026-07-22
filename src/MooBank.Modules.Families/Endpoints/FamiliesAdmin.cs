using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Families.Endpoints;

internal class FamiliesAdmin : EndpointGroupBase
{
    public override string Path => "/families/admin";

    public override string? Tag => "Families";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Family>>("/")
            .WithNames("Get All Families")
            .Produces<IEnumerable<Models.Family>>();

        routeGroupBuilder.MapQuery<Get, Models.Family>("/{id}")
            .WithNames("Get Family")
            .Produces<Models.Family>();

        routeGroupBuilder.MapPostCreate<Create, Models.Family>("/", "Get Family".ToMachine(), (i) => new { id = i.Id }, RequestBinding.Body)
            .WithNames("Create Family");

        routeGroupBuilder.MapPatchCommand<Update, Models.Family>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Family");
    }
}
