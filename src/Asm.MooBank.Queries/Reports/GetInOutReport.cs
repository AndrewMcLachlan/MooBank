using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetInOutReport : ReportQuery, IQuery<InOutReport>
{
}
