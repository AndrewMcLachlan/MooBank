using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Group.Commands;
using Asm.MooBank.Modules.Group.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Group.Endpoints;

public class Groups : EndpointGroupBase
{
    public override string Name => "Groups";

    public override string Path => "/groups";

    public override string Tags => "Groups";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Group>>("/")
            .WithName("Get All Groups")
            .Produces<IEnumerable<Models.Group>>();

        routeGroupBuilder.MapQuery<Get, Models.Group>("/{id}")
            .WithName("Get Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapPostCreate<Create, Models.Group>("/", "Get Group", (Models.Group group) => new { id = group.Id })
            .WithName("Create Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapPatchCommand<Update, Models.Group>("/{id}")
            .WithName("Update Group")
            .Produces<Models.Group>();

        routeGroupBuilder.MapDelete<Delete>("/{id}")
            .WithName("Delete Group");
    }
}
