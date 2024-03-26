namespace Asm.MooBank.Modules.Transactions.Models;
public record CreateTransaction(decimal Amount, string Description, string? Reference, DateTimeOffset TransactionTime);

