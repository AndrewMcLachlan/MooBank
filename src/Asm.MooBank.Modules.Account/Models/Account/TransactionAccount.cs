namespace Asm.MooBank.Modules.Account.Models.Account;
public record TransactionAccount : Instrument
{
    public DateOnly? LastTransaction { get; set; }

    public decimal CalculatedBalance { get; set; }
}
