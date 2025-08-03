namespace Asm.MooBank.Models;

public partial record TransactionOffsetBy
{
    public required Transaction Transaction { get; init; }

    public required decimal Amount { get; init; }
}
