﻿using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetBreakdownReport : BaseReportQuery, IQuery<BreakdownReport>
{
    public int? TagId { get; init; }

    public required ReportType ReportType { get; init; }
}
