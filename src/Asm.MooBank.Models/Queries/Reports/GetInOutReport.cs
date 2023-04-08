using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetInOutReport : BaseReportQuery, IQuery<InOutReport>
{
}
