namespace Asm.MooBank.Modules.Reports.Models;

public record UserSpendingTrendReport
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required IEnumerable<CashFlowMonth> Months { get; init; } = [];

    public required IEnumerable<Guid> IncludedAccountIds { get; init; } = [];
}

public record CashFlowMonth
{
    public required DateOnly Month { get; init; }

    public required decimal Income { get; init; }

    public required decimal Outgoings { get; init; }

    public decimal Net => Income - Outgoings;
}
