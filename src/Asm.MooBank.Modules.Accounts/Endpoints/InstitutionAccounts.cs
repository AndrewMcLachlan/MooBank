using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries.InstitutionAccounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Accounts.Endpoints;
internal class InstitutionAccounts : EndpointGroupBase
{
    public override string Name => "Accounts";

    public override string Path => "/accounts/{instrumentId}/institution-accounts";

    public override string Tags => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Get, InstitutionAccount>("/{id}")
            .WithNames("Get Institution Account");
        

        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Institution Account".ToMachine(), a => new { id = a.Id }, CommandBinding.Parameters)
            .WithNames("Create Institution Account");

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}")
            .WithNames("Update Institution Account");

        builder.MapCommand<Close, InstitutionAccount>("/{id}/close")
            .WithNames("Close Institution Account");
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
