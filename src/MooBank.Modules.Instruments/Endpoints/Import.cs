using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Import;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

internal class Import : EndpointGroupBase
{
    public override string Name => "Import";

    public override string Path => "/instruments/{instrumentId}/accounts/{accountId}/import";

    public override string Tags => "Import";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (Guid instrumentId, Guid accountId, IFormFile file, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
        {
            if (!System.IO.Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
                !file.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only CSV files are accepted.");
            }

            using Stream stream = file.OpenReadStream();
            await commandDispatcher.Dispatch(new Commands.Import.Import(instrumentId, accountId, stream), cancellationToken);
            return Results.NoContent();
        })
            .WithMetadata(new RequireAntiforgeryTokenAttribute(false))
            .WithMetadata(new RequestSizeLimitAttribute(10 * 1024 * 1024)) // 10MB
            .WithNames("Import");

        builder.MapCommand<Reprocess>("/reprocess", StatusCodes.Status204NoContent, CommandBinding.Parameters)
            .WithNames("Reprocess")
            .Produces(StatusCodes.Status204NoContent);

    }
}
