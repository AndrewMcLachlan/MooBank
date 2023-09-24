namespace Asm.MooBank.Models;
public partial record TransactionOffsetFor
{
    public required Transaction Transaction { get; init; }

    public required decimal Amount { get; init; }
}
