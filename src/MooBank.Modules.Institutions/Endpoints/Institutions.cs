using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Institutions.Commands;
using Asm.MooBank.Modules.Institutions.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Institutions.Endpoints;

internal class Institutions : EndpointGroupBase
{
    public override string Path => "/institutions";

    public override string? Tag => "Institutions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Institution>>("/")
            .WithNames("Get All Institutions");

        routeGroupBuilder.MapQuery<Get, Models.Institution>("/{id}")
            .WithNames("Get Institution");

        routeGroupBuilder.MapPostCreate<Create, Models.Institution>("/", "Get Institution".ToMachine(), (i) => new { i.Id }, RequestBinding.Body)
            .WithNames("Create Institution")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<Create>();

        routeGroupBuilder.MapPatchCommand<Update, Models.Institution>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Institution")
            .Accepts<Update>("application/json")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<Update>();
    }
}
