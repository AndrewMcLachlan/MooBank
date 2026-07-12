namespace Asm.MooBank.Models;

public partial record TransactionOffsetBy : ITransactionOffsetUpdate
{
    public required Transaction Transaction { get; init; }

    public required decimal Amount { get; init; }

    // Explicit implementation: exposes the offsetting transaction's id to the aggregate without
    // adding a member to the serialized (OpenAPI) contract.
    Guid ITransactionOffsetUpdate.OffsetTransactionId => Transaction.Id;
}
