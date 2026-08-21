using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.MooBank.Modules.Bills.Endpoints;

internal class BillReports : EndpointGroupBase
{
    public override string Path => "/bills/reports";

    public override string? Tag => "Bills";

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

internal class ChargeTypes : EndpointGroupBase
{
    public override string Path => "/bills/charge-types";

    public override string? Tag => "Bills";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<Queries.ChargeTypes.GetAll, IEnumerable<Models.ChargeType>>("/")
            .WithNames("Get Charge Types");
    }
}
