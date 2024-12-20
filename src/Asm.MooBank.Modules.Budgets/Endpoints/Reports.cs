﻿using Asm.AspNetCore;
using Asm.AspNetCore.Routing;
using Asm.MooBank.Modules.Budgets.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Asm.MooBank.Modules.Budgets.Endpoints;
public class ReportEndpoint : EndpointGroupBase
{
    public override string Name => "Budget";

    public override string Path => "/budget/{year}/report";

    public override string Tags => "Budget Report";


    protected override void MapEndpoints(IEndpointRouteBuilder routeGroupBuilder)
    {

        routeGroupBuilder.MapQuery<Report, BudgetReportByMonth>("")
            .WithNames("Get Budget Report");

        routeGroupBuilder.MapQuery<ReportForMonth, BudgetReportValueMonth?>("{month}")
            .WithNames("Get Budget Report for Month");

        routeGroupBuilder.MapQuery<ReportForMonthBreakdown, BudgetReportByMonthBreakdown?>("{month}/breakdown")
            .WithNames("Get Budget Report Breakdown for Month");

        routeGroupBuilder.MapQuery<ReportForMonthBreakdownUnbudgeted, BudgetReportByMonthBreakdown?>("{month}/breakdown/unbudgeted")
            .WithNames("Get Budget Report Breakdown for Month for Unbudgeted Items");
    }
}

