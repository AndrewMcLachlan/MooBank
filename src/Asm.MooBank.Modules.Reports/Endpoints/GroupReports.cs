using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Reports.Endpoints;

internal class GroupReports : EndpointGroupBase
{
    public override string Name => "Group Reports";

    public override string Path => "groups/{groupId}/reports";

    public override string Tags => "Group Reports";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetGroupMonthlyBalancesReport, MonthlyBalancesReport>("monthly-balances/{start}/{end}")
            .WithNames("Group Monthly Balances Report");
    }
}
