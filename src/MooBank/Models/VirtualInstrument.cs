namespace Asm.MooBank.Models;

public record VirtualInstrument : TransactionInstrument
{
    public Guid ParentId { get; set; }
}
