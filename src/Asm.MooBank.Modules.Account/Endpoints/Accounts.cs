using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Commands.InstitutionAccount;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Queries.Account;
using Asm.MooBank.Modules.Accounts.Queries.InstitutionAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        builder.MapQuery<GetFormatted, AccountsList>("/position")
            .WithNames("Get Formatted Accounts List");

        builder.MapQuery<GetList, IEnumerable<ListItem<Guid>>>("/list")
            .WithNames("Get Accounts List");

        builder.MapQuery<Get, InstitutionAccount>("/{id}")
            .WithNames("Get Account");

        builder.MapPostCreate<Create, InstitutionAccount>("/", "Get Account", a => new { a.Id }, CommandBinding.Body)
            .WithNames("Create Account");

        builder.MapPatchCommand<Update, InstitutionAccount>("/{id}")
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
