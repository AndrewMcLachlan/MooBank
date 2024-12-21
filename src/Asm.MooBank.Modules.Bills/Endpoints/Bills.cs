using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Bills.Endpoints;

internal class Bills : EndpointGroupBase
{
    public override string Name => "Bills";

    public override string Path => "/bills";

    public override string Tags => "Bills";


    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapPagedQuery<Queries.Bills.GetAll, Models.Bill>("/")
            .WithNames("Get All Bills");

        builder.MapQuery<Queries.Accounts.GetAllByType, IEnumerable<Models.Account>>("/accounts/types")
            .WithNames("Get Bill Accounts By Type");

        builder.MapQuery<Queries.Accounts.GetByType, IEnumerable<Models.Account>>("/accounts/types/{type}")
    .WithNames("Get Bill Accounts By Type");

        builder.MapQuery<Queries.Accounts.GetAll, IEnumerable<Models.Account>>("/accounts")
            .WithNames("Get Bill Accounts");

        builder.MapQuery<Queries.Accounts.Get, Models.Account>("/accounts/{instrumentId}")
            .WithNames("Get Bill Account")
            .RequireAuthorization(Policies.InstrumentViewer);

        builder.MapPagedQuery<Queries.Bills.GetForAccount, Models.Bill>("/accounts/{instrumentId}/bills");
    }
}
