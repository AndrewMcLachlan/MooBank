using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Bills.Endpoints;

internal class BillReports : EndpointGroupBase
{
    public override string Name => "Bill Reports";

    public override string Path => "/bills/reports";

    public override string Tags => "Bills";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Queries.Reports.GetCostPerUnitReport, Models.CostPerUnitReport>("/cost-per-unit")
            .WithNames("Get Cost Per Unit Report");

        builder.MapQuery<Queries.Reports.GetServiceChargeReport, Models.ServiceChargeReport>("/service-charge")
            .WithNames("Get Service Charge Report");

        builder.MapQuery<Queries.Reports.GetUsageReport, Models.UsageReport>("/usage")
            .WithNames("Get Usage Report");
    }
}
