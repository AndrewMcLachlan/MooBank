using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Commands.Recurring;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Accounts.Endpoints;
internal class RecurringEndpoints : EndpointGroupBase
{
    public override string Name => "Recurring Transactions";

    public override string Path => "accounts/{accountId}/recurring";

    public override string Tags => "Recurring Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<RecurringTransaction>>("/")
             .WithNames("Get All Recurring Transactions")
             .Produces<IEnumerable<RecurringTransaction>>();

        routeGroupBuilder.MapQuery<Get, RecurringTransaction>("/{recurringTransactionId}")
            .WithNames("Get Recurring Transaction")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapPostCreate<Create, RecurringTransaction>("", "Get Recurring Transaction".ToMachine(), (recurring) => new { recurringTransactionId = recurring.Id }, CommandBinding.None)
            .WithNames("Create Recurring Transaction")
            .Accepts<Create>("application/json")
            .Produces<RecurringTransaction>();


        routeGroupBuilder.MapPatchCommand<Update, RecurringTransaction>("/{recurringTransactionId}", CommandBinding.None)
            .WithNames("Update Recurring Transaction")
            .Accepts<Update>("application/json")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapDelete<Delete>("/{recurringTransactionId}")
            .WithNames("Delete Recurring Transaction");
    }
}
