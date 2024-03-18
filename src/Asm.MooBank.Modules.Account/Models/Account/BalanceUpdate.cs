namespace Asm.MooBank.Modules.Account.Models.Account;
public record BalanceUpdate
{
    public decimal Balance { get; init; }
    public DateOnly Date { get; init; }
    public string? Description { get; init; }
}
