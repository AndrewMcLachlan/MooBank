using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Bills.Models;

public record AccountTypeSummary
{
    public UtilityType UtilityType { get; init; }

    public DateOnly From { get; init; }

    public IEnumerable<string> Accounts { get; init; } = [];
}
