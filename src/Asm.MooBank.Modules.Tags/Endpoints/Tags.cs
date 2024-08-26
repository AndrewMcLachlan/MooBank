using Asm.AspNetCore;
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Models;
using Asm.MooBank.Modules.Tags.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Tags.Endpoints;
internal class TagsEndpoints : EndpointGroupBase
{
    public override string Name => "Tags";

    public override string Path => "/tags";

    public override string Tags => "Tags";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<MooBank.Models.Tag>>("")
            .WithNames("Get Tags");

        builder.MapQuery<GetTagsHierarchy, TagHierarchy>("hierarchy")
            .WithNames("Get Tag Hierarchy");

        builder.MapQuery<Get, MooBank.Models.Tag>("{id}")
            .WithNames("Get Tag");

        builder.MapPostCreate<Create, MooBank.Models.Tag>("", "get-tag", t => new { t.Id }, CommandBinding.Body)
            .WithNames("Create Tag");

        builder.MapPutCreate<CreateByName, MooBank.Models.Tag>("{name}", "get-tag", t => new { t.Id })
            .WithNames("Create Tag by Name")
            .WithSummary("Create a tag by name");

        builder.MapPatchCommand<Update, MooBank.Models.Tag>("{id}")
            .WithNames("Update Tag");

        builder.MapDelete<Delete>("{id}")
            .WithNames("Delete Tag");

        builder.MapPutCommand<AddSubTag, MooBank.Models.Tag>("{id}/tags/{subTagId}")
            .WithNames("Add Sub Tag");

        builder.MapDelete<RemoveSubTag>("{id}/tags/{subTagId}")
            .WithNames("Remove Sub Tag");
    }
}
