using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Modules.Instruments.Queries.Instruments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

internal class Instruments : EndpointGroupBase
{
    public override string Path => "/instruments";

    public override string? Tag => "Instruments";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetFormatted, InstrumentsList>("/summary")
            .WithNames("Get Formatted Instruments List");

        builder.MapQuery<GetList, IEnumerable<ListItem<Guid>>>("/list")
            .WithNames("Get Instruments List");
    }
}
