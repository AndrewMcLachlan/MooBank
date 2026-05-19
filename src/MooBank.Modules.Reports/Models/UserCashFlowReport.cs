namespace Asm.MooBank.Modules.Reports.Models;

public record UserCashFlowReport
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required decimal Income { get; init; }

    public required decimal Outgoings { get; init; }

    public decimal Net => Income - Outgoings;

    public required IEnumerable<Guid> IncludedAccountIds { get; init; } = [];
}
