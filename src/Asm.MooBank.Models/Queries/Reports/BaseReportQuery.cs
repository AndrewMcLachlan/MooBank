namespace Asm.MooBank.Models.Queries.Reports;

public abstract record BaseReportQuery
{
    public required Guid AccountId { get; init; }

    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }
}
