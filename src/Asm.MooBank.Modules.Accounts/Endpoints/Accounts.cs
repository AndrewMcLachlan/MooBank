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
        builder.MapQuery<GetAll, IEnumerable<InstitutionAccount>>("/")
            .WithNames("Get Accounts");

        builder.MapQuery<Get, InstitutionAccount>("/{instrumentId}")
            .WithNames("Get Account");

        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Account".ToMachine(), a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Account");

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}") // TODO: Convert to instrumentId
            .WithNames("Update Account");
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
