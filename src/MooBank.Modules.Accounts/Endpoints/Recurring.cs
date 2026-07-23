using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands.Recurring;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Accounts.Endpoints;

internal class RecurringEndpoints : EndpointGroupBase
{
    public override string Path => "accounts/{accountId}/recurring";

    public override string? Tag => "Recurring Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<RecurringTransaction>>("/")
             .WithNames("Get All Recurring Transactions")
             .Produces<IEnumerable<RecurringTransaction>>();

        routeGroupBuilder.MapQuery<Get, RecurringTransaction>("/{recurringTransactionId}")
            .WithNames("Get Recurring Transaction")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapPostCreate<Create, RecurringTransaction>("", "Get Recurring Transaction".ToMachine(), (recurring) => new { recurringTransactionId = recurring.Id }, RequestBinding.Default)
            .WithNames("Create Recurring Transaction")
            .Accepts<Create>("application/json")
            .Produces<RecurringTransaction>();


        routeGroupBuilder.MapPatchCommand<Update, RecurringTransaction>("/{recurringTransactionId}", binding: RequestBinding.Default)
            .WithNames("Update Recurring Transaction")
            .Accepts<Update>("application/json")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapDeleteCommand<Delete>("/{recurringTransactionId}")
            .WithNames("Delete Recurring Transaction");
    }
}
