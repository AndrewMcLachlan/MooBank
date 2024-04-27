using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Users.Commands;
using Asm.MooBank.Modules.Users.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Users.Endpoints;

public class AccountHolder : EndpointGroupBase
{
    public override string Name => "Account Holder";

    public override string Path => "/account-holders";

    public override string Tags => "Account Holder";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        /*routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.AccountHolder>>("/")
            .WithName("Get All Account Holder")
            .Produces<IEnumerable<Models.AccountHolder>>();

        routeGroupBuilder.MapQuery<Get, Models.AccountHolder>("/{id}")
            .WithName("Get Account Holder")
            .Produces<Models.AccountHolder>();

        routeGroupBuilder.MapPostCreate<Create, Models.AccountHolder>("/", "Get Account Holder", (Models.AccountHolder Holder) => new { id = Holder.Id })
            .WithName("Create Account Holder")
            .Produces<Models.AccountHolder>();*/

        routeGroupBuilder.MapQuery<Get, Models.AccountHolder>("/me")
            .WithNames("Get Account Holder")
            .Produces<Models.AccountHolder>();

        routeGroupBuilder.MapPatchCommand<Update, Models.AccountHolder>("/me")
            .WithNames("Update Account Holder")
            .Produces<Models.AccountHolder>();

        //routeGroupBuilder.MapDelete<Delete>("/{id}")
        //.WithName("Delete Account Holder");
    }
}
