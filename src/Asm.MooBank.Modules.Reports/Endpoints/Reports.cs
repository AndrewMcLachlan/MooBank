using Asm.Cqrs.AspNetCore;
using Asm.MooBank.Models.Reports;
using Asm.MooBank.Queries.Reports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Reports.Endpoints;
internal class Reports : EndpointGroupBase
{
    public override string Name => "Reports";

    public override string Path => "reports/{accountId}";

    public override string Tags => "Reports";

    protected override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MapQuery<GetInOutReport, InOutReport>("in-out/{start}/{end}")
            .WithNames("In-out Report");

        builder.MapQuery<GetInOutTrendReport, InOutTrendReport>("in-out-trend/{start}/{end}")
            .WithNames("In-out Trend Report");

        builder.MapQuery<GetByTagReport, ByTagReport>("{reportType}/tags/{start}/{end}/{parentTag?}")
            .WithNames("By Tag Report");

        builder.MapQuery<GetBreakdownReport, BreakdownReport>("{reportType}/breakdown/{start}/{end}/{parentTag?}")
            .WithNames("Tag Breakdown Report");

        builder.MapQuery<GetTagTrendReport, TagTrendReport>("{reportType}/tag-trend/{start}/{end}/{tag}")
            .WithNames("Tag Trend Report");

        builder.MapQuery<GetAllTagAverageReport, AllTagAverageReport>("{reportType}/all-tag-average/{start}/{end}")
            .WithNames("All Tag Average Report");

    }
}
