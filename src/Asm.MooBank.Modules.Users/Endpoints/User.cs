using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Users.Commands;
using Asm.MooBank.Modules.Users.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Users.Endpoints;

public class User : EndpointGroupBase
{
    public override string Name => "User";

    public override string Path => "/users";

    public override string Tags => "User";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        /*routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.User>>("/")
            .WithName("Get All User")
            .Produces<IEnumerable<Models.User>>();

        routeGroupBuilder.MapQuery<Get, Models.User>("/{id}")
            .WithName("Get User")
            .Produces<Models.User>();

        routeGroupBuilder.MapPostCreate<Create, Models.User>("/", "Get User", (Models.User Holder) => new { id = Holder.Id })
            .WithName("Create User")
            .Produces<Models.User>();*/

        routeGroupBuilder.MapQuery<Get, Models.User>("/me")
            .WithNames("Get User")
            .Produces<Models.User>();

        routeGroupBuilder.MapPatchCommand<Update, Models.User>("/me")
            .WithNames("Update User")
            .Produces<Models.User>();

        //routeGroupBuilder.MapDelete<Delete>("/{id}")
        //.WithName("Delete User");
    }
}
