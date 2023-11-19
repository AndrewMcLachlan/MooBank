namespace Asm.MooBank.Models.Reports;

public abstract record ReportBase
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }
}
