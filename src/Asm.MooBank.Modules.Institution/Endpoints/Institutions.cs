using Asm.Cqrs.AspNetCore;
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
            .WithName("Get All Institutions")
            .Produces<IEnumerable<Models.Institution>>();
    }
}
