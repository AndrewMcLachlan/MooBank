using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Assets.Models;
using Asm.MooBank.Modules.Assets.Commands;
using Asm.MooBank.Modules.Assets.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Assets.Endpoints;
internal class Assets : EndpointGroupBase
{
    public override string Name => "Assets";

    public override string Path => "/assets";

    public override string Tags => "assets";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, Asset>("/{id}")
            .WithNames("Get Asset");

        builder.MapPostCreate<Create, Asset>("/", "Get Asset".ToMachine(), a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Asset");

        builder.MapPatchCommand<Update, Asset>("/{id}", CommandBinding.None)
            .WithNames("Update Asset");
    }
}
