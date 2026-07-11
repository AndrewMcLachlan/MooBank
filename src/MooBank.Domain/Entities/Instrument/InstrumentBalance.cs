namespace Asm.MooBank.Domain.Entities.Instrument;

/// <summary>
/// Read-model for a <see cref="TransactionInstrument"/>'s derived values, mapped to the
/// dbo.TransactionInstrumentBalance view. Kept as a separate 1:1 entity (rather than mapping the
/// view onto TransactionInstrument directly) because TransactionInstrument participates in a TPT
/// hierarchy, and giving a TPT type a second table base (table + view) is not supported by EF Core.
/// </summary>
public class InstrumentBalance
{
    public Guid InstrumentId { get; set; }

    public decimal Balance { get; set; }

    public DateOnly? LastTransaction { get; set; }
}
