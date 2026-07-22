using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Assets.Commands;
using Asm.MooBank.Modules.Assets.Models;
using Asm.MooBank.Modules.Assets.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Assets.Endpoints;

internal class Assets : EndpointGroupBase
{
    public override string Path => "/assets";

    public override string? Tag => "Assets";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, Asset>("/{id}")
            .WithNames("Get Asset")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("id"));

        builder.MapPostCreate<Create, Asset>("/", "Get Asset".ToMachine(), a => new { a.Id }, RequestBinding.Body)
            .WithNames("Create Asset");

        builder.MapPatchCommand<Update, Asset>("/{id}", binding: RequestBinding.Default)
            .WithNames("Update Asset")
            .Accepts<Update>("application/json")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("id"));
    }
}
