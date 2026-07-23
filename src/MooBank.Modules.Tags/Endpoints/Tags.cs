using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Models;
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Tags.Endpoints;

internal class TagsEndpoints : EndpointGroupBase
{
    public override string Path => "/tags";

    public override string? Tag => "Tags";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<MooBank.Models.Tag>>("")
            .WithNames("Get Tags");

        builder.MapQuery<GetTagsHierarchy, TagHierarchy>("hierarchy")
            .WithNames("Get Tag Hierarchy");

        builder.MapQuery<GetTagsGraph, TagGraph>("graph")
            .WithNames("Get Tag Graph");

        builder.MapQuery<Get, MooBank.Models.Tag>("{id}")
            .WithNames("Get Tag")
            .RequireAuthorization(Policies.GetTagFamilyPolicy());

        builder.MapPostCreate<Create, MooBank.Models.Tag>("", "get-tag", t => new { t.Id }, RequestBinding.Body)
            .WithNames("Create Tag")
            .WithValidation<Create>();

        builder.MapPutCreate<CreateByName, MooBank.Models.Tag>("{name}", "get-tag", t => new { t.Id }, RequestBinding.Parameters)
            .WithNames("Create Tag by Name")
            .Accepts<IEnumerable<int>>("application/json")
            .WithSummary("Create a tag by name");

        builder.MapPatchCommand<Update, MooBank.Models.Tag>("{id}", binding: RequestBinding.Parameters)
            .WithNames("Update Tag")
            .WithValidation<Update>()
            .RequireAuthorization(Policies.GetTagFamilyPolicy());

        builder.MapDeleteCommand<Delete>("{id}")
            .WithNames("Delete Tag")
            .RequireAuthorization(Policies.GetTagFamilyPolicy());

        builder.MapPutCommand<AddSubTag, MooBank.Models.Tag>("{id}/tags/{subTagId}", binding: RequestBinding.Parameters)
            .WithNames("Add Sub Tag")
            .RequireAuthorization(Policies.GetTagFamilyPolicy())
            .RequireAuthorization(Policies.GetTagFamilyPolicy("subTagId"));

        builder.MapDeleteCommand<RemoveSubTag>("{id}/tags/{subTagId}")
            .WithNames("Remove Sub Tag")
            .RequireAuthorization(Policies.GetTagFamilyPolicy())
            .RequireAuthorization(Policies.GetTagFamilyPolicy("subTagId"));
    }
}
