using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.AccountGroup.Commands;
using Asm.MooBank.Modules.AccountGroup.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.AccountGroup.Endpoints;

public class AccountGroups : EndpointGroupBase
{
    public override string Name => "Account Groups";

    public override string Path => "/account-groups";

    public override string Tags => "Account Groups";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.AccountGroup>>("/")
            .WithName("Get All Account Groups")
            .Produces<IEnumerable<Models.AccountGroup>>();

        routeGroupBuilder.MapQuery<Get, Models.AccountGroup>("/{id}")
            .WithName("Get Account Group")
            .Produces<Models.AccountGroup>();

        routeGroupBuilder.MapPostCreate<Create, Models.AccountGroup>("/", "Get Account Group", (Models.AccountGroup group) => new { id = group.Id })
            .WithName("Create Account Group")
            .Produces<Models.AccountGroup>();

        routeGroupBuilder.MapPatchCommand<Update, Models.AccountGroup>("/{id}")
            .WithName("Update Account Group")
            .Produces<Models.AccountGroup>();

        routeGroupBuilder.MapDelete2<Delete>("/{id}")
            .WithName("Delete Account Group");
    }
}

public static class Ext
{
    public static RouteHandlerBuilder MapDelete2<TRequest>(this IEndpointRouteBuilder builder, string pattern) where TRequest : ICommand
    {
        return builder.MapDelete(pattern, HandleDelete2<TRequest>);
    }

    internal static async ValueTask<IResult> HandleDelete2<TRequest>([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : ICommand
    {
        await dispatcher.Dispatch(request!, cancellationToken);

        return Results.NoContent();
    }
}
