using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Commands.InstitutionAccount;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Modules.Account.Queries.InstitutionAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Account.Endpoints;
internal class Accounts : EndpointGroupBase
{
    public override string Name => "Accounts";

    public override string Path => "/accounts";

    public override string Tags => "Accounts";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetAll, IEnumerable<InstitutionAccount>>("/")
            .WithNames("Get Accounts");

        builder.MapQuery<GetFormatted, AccountsList>("/position")
            .WithNames("Get Accounts List");

        builder.MapQuery<Get, InstitutionAccount>("/{id}")
            .WithNames("Get Account");

        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Account", a => new { a.Id })
            .WithNames("Create Account");

        //builder.MapPost("/", CreateCreateHandler<Create, InstitutionAccount>("Get Account", a => new { a.Id }));

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}")
            .WithNames("Update Account");

        builder.MapPatchCommand<UpdateBalance, InstitutionAccount>("/{id}/balance")
            .WithNames("Set Balance");
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
