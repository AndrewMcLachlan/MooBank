namespace Asm.MooBank.Modules.Account.Models.Account;
public record TransactionAccount : Account
{
    public DateOnly? LastTransaction { get; set; }

    public decimal CalculatedBalance { get; set; }
}
