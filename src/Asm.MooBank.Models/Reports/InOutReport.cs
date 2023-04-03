namespace Asm.MooBank.Models.Reports;

public record InOutReport
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required decimal Income { get; init; }

    public required decimal Outgoings { get; init; }

}
