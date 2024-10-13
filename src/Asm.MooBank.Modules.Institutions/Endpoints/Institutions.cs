using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Institutions.Commands;
using Asm.MooBank.Modules.Institutions.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Institutions.Endpoints;
internal class Institutions : EndpointGroupBase
{
    public override string Name => "Institutions";

    public override string Path => "/institutions";

    public override string Tags => "Institutions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Institution>>("/")
            .WithNames("Get All Institutions");

        routeGroupBuilder.MapQuery<Get, Models.Institution>("/{id}")
            .WithNames("Get Institution");

        routeGroupBuilder.MapPostCreate<Create, Models.Institution>("/", "Get Institution".ToMachine(), (i) => new { i.Id })
            .WithNames("Create Institution")
            .RequireAuthorization(Policies.Admin);

        routeGroupBuilder.MapPatchCommand<Update, Models.Institution>("/{id}")
            .WithNames("Update Institution")
            .RequireAuthorization(Policies.Admin);
    }
}
