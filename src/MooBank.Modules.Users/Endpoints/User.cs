using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Users.Commands;
using Asm.MooBank.Modules.Users.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Users.Endpoints;

public class User : EndpointGroupBase
{
    public override string Path => "/users";

    public override string? Tag => "User";


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

        routeGroupBuilder.MapPatchCommand<Update, Models.User>("/me", binding: RequestBinding.Parameters)
            .WithNames("Update User")
            .WithValidation<Update>()
            .Produces<Models.User>();

        //routeGroupBuilder.MapDeleteCommand<Delete>("/{id}")
        //.WithName("Delete User");
    }
}
