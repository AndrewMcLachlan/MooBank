using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
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

        builder.MapPostCreate<Commands.Accounts.Create, Models.Account>("/accounts", "Get Bill Account".ToMachine(), a => new { InstrumentId = a.Id })
            .WithNames("Create Bill Account");

        builder.MapPagedQuery<Queries.Bills.GetByUtilityType, Models.Bill>("/types/{utilityType}/bills")
            .WithNames("Get Bills By Utility Type");

        builder.MapCommand<Commands.Bills.Import, Models.ImportResult>("/import", CommandBinding.Body)
            .WithNames("Import Bills");
    }
}
