using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Bills.Endpoints;

internal class BillAccounts : EndpointGroupBase
{
    public override string Name => "Bill Accounts";

    public override string Path => "/bills/accounts/{instrumentId}";

    public override string Tags => "Bills";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Queries.Accounts.Get, Models.Account>("/")
            .WithNames("Get Bill Account");

        builder.MapPagedQuery<Queries.Bills.GetForAccount, Models.Bill>("/bills")
            .WithNames("Get Bills For An Account");

        builder.MapQuery<Queries.Bills.Get, Models.Bill>("/bills/{id}")
            .WithNames("Get Bill");

        builder.MapPostCreate<Commands.Bills.Create, Models.Bill>("/bills", "Get Bill".ToMachine(), b => b.Id, CommandBinding.Parameters)
            .WithNames("Create Bill")
            .RequireAuthorization(Policies.GetInstrumentOwnerPolicy());
    }
}
