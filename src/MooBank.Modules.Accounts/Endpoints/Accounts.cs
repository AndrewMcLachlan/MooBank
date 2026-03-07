using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Accounts.Endpoints;

internal class Accounts : EndpointGroupBase
{
    public override string Name => "Accounts";

    public override string Path => "/accounts";

    public override string Tags => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<LogicalAccount>>("/")
            .WithNames("Get Accounts");

        builder.MapQuery<Get, LogicalAccount>("/{instrumentId}")
            .WithNames("Get Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapPostCreate<Create, LogicalAccount>("/", "Get Account".ToMachine(), a => new { instrumentId = a.Id }, CommandBinding.Body)
            .WithNames("Create Account")
            .WithValidation<Create>();

        builder.MapPatchCommand<Update, LogicalAccount>("/{id}")
            .WithNames("Update Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy("id"))
            .WithValidation<Update>();
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
