namespace Asm.MooBank.Institution.AustralianSuper.Domain;

/// <summary>
/// A lightweight projection of <see cref="TransactionRaw"/> used for duplicate detection during import.
/// </summary>
internal sealed record TransactionRawSummary(string? Description, DateOnly Date, decimal TotalAmount);
