namespace Asm.MooBank.Modules.Instruments.Models.Virtual;

public record CreateVirtualInstrument
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal OpeningBalance { get; set; } = 0;
}
