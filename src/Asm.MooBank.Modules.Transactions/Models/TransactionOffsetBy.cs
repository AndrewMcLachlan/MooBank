namespace Asm.MooBank.Modules.Transactions.Models;

public partial record TransactionOffsetBy
{
    public required Transaction Transaction { get; init; }

    public required decimal Amount { get; init; }
}
