using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Families.Endpoints;

internal class Families : EndpointGroupBase
{
    public override string Path => "/families";

    public override string? Tag => "Families";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetMine, Models.Family>("/")
            .WithNames("Get My Family")
            .Produces<Models.Family>();

        routeGroupBuilder.MapPatchCommand<UpdateMine, Models.Family>("/", binding: RequestBinding.Parameters)
            .WithNames("Update My Family")
            .Produces<Models.Family>();

        routeGroupBuilder.MapDeleteCommand<RemoveMember>("/members/{userId}")
            .WithNames("Remove Family Member");
    }
}
