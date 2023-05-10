using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Queries.Reports;

public record GetInOutReport : ReportQuery, IQuery<InOutReport>
{
}
