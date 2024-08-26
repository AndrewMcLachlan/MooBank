using Asm.AspNetCore;
using Asm.MooBank.Modules.ReferenceData.Models;
using Asm.MooBank.Modules.ReferenceData.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.ReferenceData.Endpoints;
internal class ReferenceData : EndpointGroupBase
{
    public override string Name => "Reference Data";

    public override string Path => "/reference-data";

    public override string Tags => "Reference Data";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetImporterTypes, IEnumerable<ImporterType>>("importer-types")
            .WithNames("Importer Types");
    }
}
