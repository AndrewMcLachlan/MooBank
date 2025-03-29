using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Groups.Endpoints;

public class Groups : EndpointGroupBase
{
    public override string Name => "Groups";

    public override string Path => "/groups";

    public override string Tags => "Groups";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Group>>("/")
            .WithNames("Get All Groups")
            .Produces<IEnumerable<Models.Group>>();

        routeGroupBuilder.MapQuery<Get, Models.Group>("/{id}")
            .WithNames("Get Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapPostCreate<Create, Models.Group>("/", "Get Group".ToMachine(), (group) => new { id = group.Id })
            .WithNames("Create Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapPatchCommand<Update, Models.Group>("/{id}")
            .WithNames("Update Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapDelete<Delete>("/{id}")
            .WithNames("Delete Group");
    }
}
