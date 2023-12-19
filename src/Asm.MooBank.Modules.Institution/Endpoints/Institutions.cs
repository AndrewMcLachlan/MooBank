using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Institution.Commands;
using Asm.MooBank.Modules.Institution.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Institution.Endpoints;
internal class Institutions : EndpointGroupBase
{
    public override string Name => "Institutions";

    public override string Path => "/institutions";

    public override string Tags => "Institutions";

    public override string AuthorisationPolicy => Policies.Admin;

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Institution>>("/")
            .WithNames("Get All Institutions")
            .Produces<IEnumerable<Models.Institution>>();

        routeGroupBuilder.MapQuery<Get, Models.Institution>("/{id}")
            .WithNames("Get Institution")
            .Produces<Models.Institution>();

        routeGroupBuilder.MapPostCreate<Create, Models.Institution>("/", "Get Institution".ToMachine(), (i) => new { i.Id })
            .WithNames("Create Institution");

        routeGroupBuilder.MapPatchCommand<Update, Models.Institution>("/{id}")
            .WithNames("Update Institution");
    }
}
