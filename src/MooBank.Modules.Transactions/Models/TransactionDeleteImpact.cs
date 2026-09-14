namespace Asm.MooBank.Modules.Transactions.Models;

/// <summary>
/// What a hard delete of a transaction would take with it, so the user can be told before it happens.
/// </summary>
public record TransactionDeleteImpact
{
    /// <summary>
    /// Transactions this one offsets. Deleting it puts their net amounts back up.
    /// </summary>
    public required IEnumerable<TransactionDeleteImpactRefund> Refunds { get; init; }

    /// <summary>
    /// Planned items that have claimed this transaction as one of their actual payments.
    /// </summary>
    public required IEnumerable<TransactionDeleteImpactPlannedItem> PlannedItems { get; init; }
}

public record TransactionDeleteImpactRefund(decimal Amount, string? Description);

public record TransactionDeleteImpactPlannedItem(string PlanName, string ItemName);
