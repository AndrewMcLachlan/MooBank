using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Groups.Endpoints;

public class Groups : EndpointGroupBase
{
    public override string Path => "/groups";

    public override string? Tag => "Groups";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Group>>("/")
            .WithNames("Get All Groups");

        routeGroupBuilder.MapQuery<Get, Models.Group>("/{id}")
            .WithNames("Get Group")
            .RequireAuthorization(Policies.GetGroupOwnerPolicy("id"));

        routeGroupBuilder.MapPostCreate<Create, Models.Group>("/", "Get Group".ToMachine(), (group) => new { id = group.Id }, RequestBinding.Body)
            .WithNames("Create Group")
            .WithValidation<Create>();

        // No id in the route, so no parameterized policy applies: the handler answers only for the
        // caller's own groups, and asserts ownership of each before moving it.
        routeGroupBuilder.MapPutCommand<Reorder, IEnumerable<Models.Group>>("/order", binding: RequestBinding.Body)
            .WithNames("Reorder Groups");

        routeGroupBuilder.MapPatchCommand<Update, Models.Group>("/{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Group")
            .RequireAuthorization(Policies.GetGroupOwnerPolicy("id"))
            .WithValidation<Update>();

        routeGroupBuilder.MapDeleteCommand<Delete>("/{id}")
            .WithNames("Delete Group")
            .RequireAuthorization(Policies.GetGroupOwnerPolicy("id"));
    }
}
