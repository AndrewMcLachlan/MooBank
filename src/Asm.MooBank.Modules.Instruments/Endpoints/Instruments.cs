using Asm.AspNetCore;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Account;
using Asm.MooBank.Modules.Instruments.Queries.Instrument;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Instruments.Endpoints;
internal class Instruments : EndpointGroupBase
{
    public override string Name => "Instruments";

    public override string Path => "/instruments";

    public override string Tags => "Instruments";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetFormatted, InstrumentsList>("/summary")
            .WithNames("Get Formatted Instruments List");

        builder.MapQuery<GetList, IEnumerable<ListItem<Guid>>>("/list")
            .WithNames("Get Instruments List");
    }

    internal static Delegate CreateCreateHandler<TRequest, TResult>(string routeName, Func<TResult, object> getRouteParams) where TRequest : ICommand<TResult>
    {
        return async ([AsParameters] TRequest request, ICommandDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.Dispatch(request!, cancellationToken);

            return Results.CreatedAtRoute(routeName, getRouteParams(result), result);
        };
    }
}
