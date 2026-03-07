using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Accounts.Models.Recurring;
using Asm.MooBank.Modules.Accounts.Queries.Recurring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Accounts.Endpoints;

internal class VirtualRecurringEndpoints : EndpointGroupBase
{
    public override string Name => "Recurring Transactions";

    public override string Path => "accounts/{accountId}/virtual/{virtualAccountId}/recurring";

    public override string Tags => "Recurring Transactions";

    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {

        routeGroupBuilder.MapQuery<GetForVirtual, IEnumerable<RecurringTransaction>>("/")
            .WithName("Get Recurring Transactions for a Virtual Account");
    }
}
