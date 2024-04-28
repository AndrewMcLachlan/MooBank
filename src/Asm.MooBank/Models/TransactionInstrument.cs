namespace Asm.MooBank.Models;

public record TransactionInstrument : Instrument
{
    public DateOnly? LastTransaction { get; set; }

    public decimal CalculatedBalance { get; set; }
}
