using Asm.MooBank.Models;

namespace Asm.MooBank.Abs;

public record CpiChange
{
    public Quarter Quarter { get; init; }

    public decimal ChangePercent { get; set; } = default!;
}
