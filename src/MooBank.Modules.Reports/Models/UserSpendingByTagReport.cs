namespace Asm.MooBank.Modules.Reports.Models;

public record UserSpendingByTagReport
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required IEnumerable<TagValue> Tags { get; init; } = [];

    public required IEnumerable<Guid> IncludedAccountIds { get; init; } = [];
}
