using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Import;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;
internal class Import : EndpointGroupBase
{
    public override string Name => "Import";

    public override string Path => "/instruments/{instrumentId}/import";

    public override string Tags => "Import";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (Guid instrumentId, IFormFile file, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
        {
            using Stream stream = file.OpenReadStream();
            await commandDispatcher.Dispatch(new Commands.Import.Import(instrumentId, stream), cancellationToken);
            return Results.NoContent();
        })
            .WithMetadata(new RequireAntiforgeryTokenAttribute(false))
            .WithNames("Import");

        builder.MapCommand<Reprocess>("/reprocess", StatusCodes.Status204NoContent, CommandBinding.Parameters)
            .WithNames("Reprocess")
            .Produces(StatusCodes.Status204NoContent);

    }
}
