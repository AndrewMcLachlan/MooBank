using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Modules.Account.Models.Recurring;
using Asm.MooBank.Modules.Account.Commands.Recurring;
using Asm.MooBank.Modules.Account.Queries.Recurring;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Account.Endpoints;
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

        routeGroupBuilder.MapPostCreate<Create, RecurringTransaction>("", "Get Recurring Transaction".ToMachine(), (RecurringTransaction recurring) => new { recurringTransactionId = recurring.Id }, CommandBinding.None)
            .WithNames("Create Recurring Transaction")
            .Produces<RecurringTransaction>();


        routeGroupBuilder.MapPatchCommand<Update, RecurringTransaction>("/{recurringTransactionId}", CommandBinding.None)
            .WithNames("Update Recurring Transaction")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapDelete<Delete>("/{recurringTransactionId}")
            .WithNames("Delete Recurring Transaction");
    }
}
