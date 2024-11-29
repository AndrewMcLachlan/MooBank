namespace Asm.MooBank.Modules.Instruments.Models.Rules;
public record CreateRule
{
    public required Guid InstrumentId { get; set; }

    public required string Contains { get; set; }

    public string? Description { get; set; }

    public required IEnumerable<int> TagIds { get; set; }
}
