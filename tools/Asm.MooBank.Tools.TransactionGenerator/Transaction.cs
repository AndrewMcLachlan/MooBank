namespace Asm.MooBank.Tools.TransactionGenerator;

/// <summary>
/// Represents a generated transaction for CSV output.
/// </summary>
public class Transaction
{
    public required DateTime Date { get; init; }
    public required string Description { get; init; }
    public decimal? Credit { get; init; }
    public decimal? Debit { get; init; }
    public required decimal Balance { get; init; }

    // Metadata for tracking (not output to CSV)
    public string? Category { get; init; }
    public string? Merchant { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }

    /// <summary>
    /// Gets the absolute amount of the transaction.
    /// </summary>
    public decimal Amount => Credit ?? Debit ?? 0;

    /// <summary>
    /// Writes the transaction to CSV format.
    /// </summary>
    public string ToCsv()
    {
        var creditStr = Credit.HasValue ? Credit.Value.ToString("F2") : String.Empty;
        var debitStr = Debit.HasValue ? (-Debit.Value).ToString("F2") : String.Empty;
        return $"{Date:dd/MM/yyyy},{Description},{creditStr},{debitStr},{Balance:F2}";
    }
}

/// <summary>
/// Represents a pending transaction that will be generated (for scheduling follow-ups).
/// </summary>
public class PendingTransaction
{
    public required DateTime Date { get; init; }
    public required TransactionTemplate Template { get; init; }
    public string? SpecificMerchant { get; init; }
}
