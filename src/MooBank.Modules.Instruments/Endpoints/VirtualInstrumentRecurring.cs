using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Instruments.Commands.Recurring;
using Asm.MooBank.Modules.Instruments.Models.Recurring;
using Asm.MooBank.Modules.Instruments.Queries.Recurring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Instruments.Endpoints;

internal class VirtualInstrumentRecurringEndpoints : EndpointGroupBase
{
    public override string Path => "instruments/{instrumentId}/virtual/{virtualInstrumentId}/recurring";

    public override string? Tag => "Recurring Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {
        routeGroupBuilder.MapQuery<GetAll, IEnumerable<RecurringTransaction>>("/")
            .WithNames("Get Recurring Transactions")
            .Produces<IEnumerable<RecurringTransaction>>();

        routeGroupBuilder.MapQuery<Get, RecurringTransaction>("/{recurringTransactionId}")
            .WithNames("Get Recurring Transaction")
            .Produces<RecurringTransaction>();

        // Parameters binding: the ids come from the route and only the mutable fields are
        // carried in the body (see RecurringTransactionDetails), so the generated request
        // schema doesn't duplicate the route ids.
        routeGroupBuilder.MapPostCreate<Create, RecurringTransaction>("/", "Get Recurring Transaction".ToMachine(), (recurring) => new { recurringTransactionId = recurring.Id }, RequestBinding.Parameters)
            .WithNames("Create Recurring Transaction")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapPatchCommand<Update, RecurringTransaction>("/{recurringTransactionId}", binding: RequestBinding.Parameters)
            .WithNames("Update Recurring Transaction")
            .Produces<RecurringTransaction>();

        routeGroupBuilder.MapDeleteCommand<Delete>("/{recurringTransactionId}")
            .WithNames("Delete Recurring Transaction");
    }
}
