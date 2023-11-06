using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Commands.Import;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;
internal class Import : EndpointGroupBase
{
    public override string Name => "Import";

    public override string Path => "/accounts/{accountId}/import";

    public override string Tags => "Import";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", async (Guid accountId, IFormFile file, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
        {
            using Stream stream = file.OpenReadStream();
            await commandDispatcher.Dispatch(new Commands.Import.Import(accountId, stream), cancellationToken);
            return Results.NoContent();
        })
            .WithNames("Import Transactions");

        builder.MapCommand<Reprocess>("/reprocess", StatusCodes.Status204NoContent)
            .WithNames("Reprocess Transactions")
            .Produces(StatusCodes.Status204NoContent);

    }
}
