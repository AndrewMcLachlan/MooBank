﻿using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Families.Endpoints;
internal class Families : EndpointGroupBase
{
    public override string Name => "Families";

    public override string Path => "/families";

    public override string Tags => "Families";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<Models.Family>>("/")
            .WithNames("Get All Families")
            .Produces<IEnumerable<Models.Family>>();

        routeGroupBuilder.MapQuery<Get, Models.Family>("/{id}")
            .WithNames("Get Family")
            .Produces<Models.Family>();


        routeGroupBuilder.MapPostCreate<Create, Models.Family>("/", "Get Family".ToMachine(), (i) => new { id = i.Id }, CommandBinding.Body)
            .WithNames("Create Family")
            .RequireAuthorization(Policies.Admin);

        routeGroupBuilder.MapPatchCommand<Update, Models.Family>("/{id}")
            .WithNames("Update Family")
            .RequireAuthorization(Policies.Admin);
    }
}
