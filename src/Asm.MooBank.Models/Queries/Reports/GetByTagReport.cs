using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetByTagReport : TypedReportQuery, IQuery<ByTagReport>
{
    public int? TagId { get; init; }
}
