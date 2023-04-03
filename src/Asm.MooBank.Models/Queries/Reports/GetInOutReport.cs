using Asm.Cqrs.Queries;
using Asm.MooBank.Models.Reports;

namespace Asm.MooBank.Models.Queries.Reports;

public record GetInOutReport : IQuery<InOutReport>
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }
}
