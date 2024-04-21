using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Asset.Commands;
using Asm.MooBank.Modules.Asset.Models;
using Asm.MooBank.Modules.Asset.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Asset.Endpoints;
internal class Assets : EndpointGroupBase
{
    public override string Name => "Assets";

    public override string Path => "/assets";

    public override string Tags => "assets";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, Models.Asset>("/{id}")
            .WithNames("Get Asset");

        builder.MapPostCreate<Create, Models.Asset>("/", "Get Asset".ToMachine(), a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Asset");

        builder.MapPatchCommand<Update, Models.Asset>("/{id}", CommandBinding.None)
            .WithNames("Update Asset");
    }
}
