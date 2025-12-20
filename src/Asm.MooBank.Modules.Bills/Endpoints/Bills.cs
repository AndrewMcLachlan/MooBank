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

        builder.MapQuery<Queries.Accounts.GetAllByType, IEnumerable<Models.AccountTypeSummary>>("/accounts/types")
            .WithNames("Get Bill Account Summaries By Type");

        builder.MapQuery<Queries.Accounts.GetByType, IEnumerable<Models.Account>>("/accounts/types/{type}")
            .WithNames("Get Bill Accounts By Type");

        builder.MapQuery<Queries.Accounts.GetAll, IEnumerable<Models.Account>>("/accounts")
            .WithNames("Get Bill Accounts");

        builder.MapQuery<Queries.Accounts.Get, Models.Account>("/accounts/{instrumentId}")
            .WithNames("Get Bill Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapPagedQuery<Queries.Bills.GetForAccount, Models.Bill>("/accounts/{instrumentId}/bills")
            .WithNames("Get Bills For An Account")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapQuery<Queries.Bills.Get, Models.Bill>("/accounts/{instrumentId}/bills/{id}")
            .WithNames("Get Bill")
            .RequireAuthorization(Policies.GetInstrumentViewerPolicy());

        builder.MapPostCreate<Commands.Bills.Create, Models.Bill>("/accounts/{instrumentId}/bills", "Get Bill".ToMachine(), b => b.Id)
            .WithNames("Create Bill")
            .RequireAuthorization(Policies.GetInstrumentOwnerPolicy());
    }
}
